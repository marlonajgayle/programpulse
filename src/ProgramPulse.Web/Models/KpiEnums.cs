namespace ProgramPulse.Web.Models;

// Mirrors ProgramPulse.Api KPI enums. Duplicated here so the WASM client stays
// standalone — there is no shared assembly reference between Web and Api today.

public enum KpiDirection
{
    Increase,
    Decrease,
}

public enum KpiStatus
{
    NotStarted,
    OnTrack,
    AtRisk,
    OffTrack,
    Completed,
}

/// <summary>Programme lifecycle status, derived from the end date server-side.
/// Ordinals must match ProgramPulse.Api's ProgrammeStatus.</summary>
public enum ProgrammeStatus
{
    Active,
    Archived,
}

/// <summary>How close a KPI is to its due date — drives the due-date badge colour.</summary>
public enum KpiDueUrgency
{
    OnSchedule,
    DueSoon,
    Overdue,
}

/// <summary>How often a KPI is expected to be measured. Ordinals must match
/// ProgramPulse.Api's MeasurementFrequency.</summary>
public enum MeasurementFrequency
{
    Weekly,
    Monthly,
    Quarterly,
    BiAnnually,
    Annually,
}

/// <summary>
/// Central tuning constants for KPI card presentation. Kept in one place so the
/// overdue window, "due soon" window, and sparkline density are each a one-line
/// change rather than magic numbers scattered through the UI.
/// </summary>
public static class KpiThresholds
{
    /// <summary>A KPI whose latest measurement is older than this many days is flagged
    /// as overdue for a fresh reading.</summary>
    public const int OverdueMeasurementDays = 14;

    /// <summary>The due-date badge shifts from neutral to warn once the due date is
    /// within this many days (and to danger once it has passed).</summary>
    public const int DueSoonDays = 30;

    /// <summary>Maximum number of recent measurements plotted in the sparkline,
    /// taken most-recent-N and drawn oldest → newest, left → right.</summary>
    public const int SparklinePoints = 8;
}
