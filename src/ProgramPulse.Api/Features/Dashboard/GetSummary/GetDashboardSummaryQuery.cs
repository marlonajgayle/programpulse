using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Dashboard.GetSummary;

public sealed record GetDashboardSummaryQuery;

/// <summary>
/// Aggregates the caller's tenant into a single decision-oriented portfolio snapshot.
/// Tenant isolation is the manual <c>Where(TenantId == …)</c> filter used across the codebase;
/// soft-deleted rows are excluded automatically by the global query filter.
/// </summary>
/// <remarks>
/// The domain stores only the *current* <see cref="KpiStatus"/> (no status history) and has no
/// Review entity, so the 8-week trend, today's reviews, and the week-over-week deltas cannot be
/// computed from real data. Those fields return a best-effort current snapshot / empties here.
/// Making them real requires a status-history table and a Review entity (tracked as follow-up).
/// </remarks>
public sealed class GetDashboardSummaryQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private const int OverdueThresholdDays = 14;

    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<DashboardSummaryResponse>> HandleAsync(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<DashboardSummaryResponse>.Failure(tenant.Error);

        // Materialize the tenant's sub-tree; the worst-of roll-up and overdue checks below
        // are simpler (and clearer) evaluated in memory than translated to SQL.
        var programmes = await _dbContext.Programmes
            .AsNoTracking()
            .Where(i => i.TenantId == tenant.Value)
            .Include(i => i.Objectives)
                .ThenInclude(o => o.Kpis)
                    .ThenInclude(k => k.Measurements)
            .ToListAsync(cancellationToken);

        var teamName = await _dbContext.Tenants
            .AsNoTracking()
            .Where(t => t.Id == tenant.Value)
            .Select(t => t.Name)
            .FirstOrDefaultAsync(cancellationToken) ?? "Your team";

        var now = DateTime.UtcNow;
        var allKpis = programmes.SelectMany(i => i.Objectives).SelectMany(o => o.Kpis).ToList();

        // --- Status roll-up per programme (worst-of) ---
        var onTrack = 0;
        var atRisk = 0;
        var offTrack = 0;
        foreach (var programme in programmes)
        {
            var status = RollUp(programme.Objectives.SelectMany(o => o.Kpis));
            switch (status)
            {
                case KpiStatus.OffTrack: offTrack++; break;
                case KpiStatus.AtRisk: atRisk++; break;
                default: onTrack++; break;
            }
        }

        // --- Health = % of KPIs on track ---
        var onTrackKpis = allKpis.Count(k => k.Status == KpiStatus.OnTrack);
        var healthScore = allKpis.Count == 0
            ? 0
            : (int)Math.Round((double)onTrackKpis / allKpis.Count * 100);

        // --- Overdue: no measurement in > 14 days (or none) ---
        var overdue = allKpis.Count(k => IsOverdue(k, now));

        // --- Flagged programmes, worst-first, with their worst KPI as failing metric ---
        var flagged = programmes
            .Select(i => new
            {
                Programme = i,
                Status = RollUp(i.Objectives.SelectMany(o => o.Kpis)),
            })
            .Where(x => x.Status is KpiStatus.AtRisk or KpiStatus.OffTrack)
            .OrderByDescending(x => x.Status == KpiStatus.OffTrack)
            .Select(x => ToFlagged(x.Programme, x.Status, teamName))
            .ToList();

        // --- Velocity (computable parts only) ---
        var coverage = allKpis.Count == 0
            ? 0
            : (int)Math.Round(
                (double)allKpis.Count(k => k.Measurements.Count > 0) / allKpis.Count * 100);
        var velocity = new VelocityStats(
            KpisHitTargetThisWeek: allKpis.Count(k => k.Status == KpiStatus.Completed),
            KpisNewlySlipped: 0,            // TODO: needs status-history to know "newly" slipped
            MeasurementCoveragePercent: coverage,
            AvgDaysToTargetClose: 0);       // TODO: needs status-history (no close timestamp today)

        // TODO: needs status-history — only the current week is real; earlier weeks are empty.
        var trend = new List<TrendPoint>
        {
            new(DateOnly.FromDateTime(now), onTrack, atRisk, offTrack),
        };

        // TODO: needs a Review entity — no scheduled-review data exists in the domain yet.
        var reviews = new List<UpcomingReview>();

        var response = new DashboardSummaryResponse(
            TeamName: teamName,
            ActiveProgrammeCount: programmes.Count,
            GeneratedAtUtc: now,
            HealthScore: healthScore,
            HealthDeltaPercent: 0,          // TODO: needs status-history for week-over-week delta
            Status: new StatusCounts(onTrack, atRisk, offTrack, programmes.Count),
            AtRiskDeltaSinceLastWeek: 0,    // TODO: needs status-history
            OverdueKpiCount: overdue,
            TotalKpiCount: allKpis.Count,
            Flagged: flagged,
            Trend: trend,
            Reviews: reviews,
            Velocity: velocity);

        return Result<DashboardSummaryResponse>.Success(response);
    }

    /// <summary>Worst-of roll-up: any OffTrack → OffTrack, else any AtRisk → AtRisk, else OnTrack.</summary>
    private static KpiStatus RollUp(IEnumerable<Kpi> kpis)
    {
        var list = kpis.ToList();
        if (list.Count == 0)
            return KpiStatus.NotStarted;
        if (list.Any(k => k.Status == KpiStatus.OffTrack))
            return KpiStatus.OffTrack;
        if (list.Any(k => k.Status == KpiStatus.AtRisk))
            return KpiStatus.AtRisk;
        return list.All(k => k.Status == KpiStatus.Completed) ? KpiStatus.Completed : KpiStatus.OnTrack;
    }

    private static bool IsOverdue(Kpi kpi, DateTime now)
    {
        if (kpi.Measurements.Count == 0)
            return true;
        var latest = kpi.Measurements.Max(m => m.CreatedDate);
        return (now - latest).TotalDays > OverdueThresholdDays;
    }

    private static FlaggedProgramme ToFlagged(Programme programme, KpiStatus status, string teamName)
    {
        var kpis = programme.Objectives.SelectMany(o => o.Kpis).ToList();
        // Worst KPI = one matching the programme's rolled-up status, else the first.
        var worst = kpis.FirstOrDefault(k => k.Status == status) ?? kpis.FirstOrDefault();

        return new FlaggedProgramme(
            Id: programme.Id,
            Name: programme.Name,
            StatusModifier: status == KpiStatus.OffTrack ? "off" : "warn",
            Team: teamName,
            Owner: programme.CreatedBy,
            FailingMetric: worst?.Name ?? "—",
            CurrentValue: worst?.CurrentValue ?? 0,
            TargetValue: worst?.TargetValue ?? 0,
            PercentToTarget: worst is null ? 0 : ProgressPercent(worst));
    }

    /// <summary>
    /// How far CurrentValue has travelled from BaselineValue toward TargetValue, 0–100.
    /// Mirrors KpiVm.ProgressPercent on the frontend.
    /// </summary>
    private static int ProgressPercent(Kpi kpi)
    {
        var span = kpi.TargetValue - kpi.BaselineValue;
        if (span == 0)
            return kpi.CurrentValue == kpi.TargetValue ? 100 : 0;
        var fraction = (double)((kpi.CurrentValue - kpi.BaselineValue) / span);
        return (int)Math.Round(Math.Clamp(fraction, 0d, 1d) * 100);
    }
}
