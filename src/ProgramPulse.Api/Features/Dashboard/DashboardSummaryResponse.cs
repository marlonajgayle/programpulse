namespace ProgramPulse.Api.Features.Dashboard;

/// <summary>
/// Decision-oriented portfolio snapshot powering the stakeholder dashboard.
/// Returned by a single aggregate endpoint so the page makes one call, not six.
/// </summary>
public sealed record DashboardSummaryResponse(
    string TeamName,
    int ActiveProgrammeCount,
    DateTime GeneratedAtUtc,
    int HealthScore,
    double HealthDeltaPercent,
    StatusCounts Status,
    int AtRiskDeltaSinceLastWeek,
    int OverdueKpiCount,
    int TotalKpiCount,
    IReadOnlyList<FlaggedProgramme> Flagged,
    IReadOnlyList<TrendPoint> Trend,
    IReadOnlyList<UpcomingReview> Reviews,
    VelocityStats Velocity);

/// <summary>Programme counts by rolled-up status. Total is all programmes.</summary>
public sealed record StatusCounts(int OnTrack, int AtRisk, int OffTrack, int Total);

/// <summary>
/// An programme needing attention, with the single worst KPI surfaced as the
/// failing metric. <paramref name="StatusModifier"/> is the CSS suffix (warn|off).
/// </summary>
public sealed record FlaggedProgramme(
    Guid Id,
    string Name,
    string StatusModifier,
    string Team,
    string Owner,
    string FailingMetric,
    decimal CurrentValue,
    decimal TargetValue,
    int PercentToTarget);

/// <summary>One weekly snapshot of programme status counts for the trend chart.</summary>
public sealed record TrendPoint(DateOnly WeekStart, int OnTrack, int AtRisk, int OffTrack);

/// <summary>A scheduled review with a context line stating why it matters.</summary>
public sealed record UpcomingReview(string Time, string Title, string Context);

/// <summary>Leading-indicator velocity stats for the steering call.</summary>
public sealed record VelocityStats(
    int KpisHitTargetThisWeek,
    int KpisNewlySlipped,
    int MeasurementCoveragePercent,
    double AvgDaysToTargetClose);
