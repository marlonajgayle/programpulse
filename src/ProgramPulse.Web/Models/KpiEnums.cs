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
