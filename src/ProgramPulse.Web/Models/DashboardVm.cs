namespace ProgramPulse.Web.Models;

// View models mirroring the API's DashboardSummaryResponse (and its nested records) so the
// dashboard renders from one payload. Driven by SampleData.GetDashboardSummary today; swap
// for a single GET /api/v1/dashboard call once the WASM client is wired to the API.

public sealed record DashboardSummaryVm(
    string TeamName,
    int ActiveProgrammeCount,
    DateTime GeneratedAtUtc,
    int HealthScore,
    double HealthDeltaPercent,
    StatusCountsVm Status,
    int AtRiskDeltaSinceLastWeek,
    int OverdueKpiCount,
    int TotalKpiCount,
    IReadOnlyList<FlaggedProgrammeVm> Flagged,
    IReadOnlyList<TrendPointVm> Trend,
    IReadOnlyList<UpcomingReviewVm> Reviews,
    VelocityStatsVm Velocity)
{
    public int OnTrackPercent => Status.Total == 0 ? 0 : (int)Math.Round((double)Status.OnTrack / Status.Total * 100);

    public int MinutesAgo => Math.Max(0, (int)Math.Round((DateTime.UtcNow - GeneratedAtUtc).TotalMinutes));
}

public sealed record StatusCountsVm(int OnTrack, int AtRisk, int OffTrack, int Total);

public sealed record FlaggedProgrammeVm(
    Guid Id,
    string Name,
    string StatusModifier,   // "warn" | "off"
    string Team,
    string Owner,
    string FailingMetric,
    decimal CurrentValue,
    decimal TargetValue,
    int PercentToTarget);

public sealed record TrendPointVm(DateOnly WeekStart, int OnTrack, int AtRisk, int OffTrack);

public sealed record UpcomingReviewVm(string Time, string Title, string Context, string StatusModifier);

public sealed record VelocityStatsVm(
    int KpisHitTargetThisWeek,
    int KpisNewlySlipped,
    int MeasurementCoveragePercent,
    int AvgDaysToTargetClose);
