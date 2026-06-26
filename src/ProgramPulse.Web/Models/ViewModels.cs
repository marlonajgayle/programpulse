namespace ProgramPulse.Web.Models;

// View models mirroring the API response DTOs (InitiativeResponse, ObjectiveResponse,
// KpiResponse, MeasurementResponse) but carrying their child collections so a page can
// render the whole sub-tree from mock data. Swap SampleData for an API-backed source later.

public sealed record InitiativeVm(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    IReadOnlyList<ObjectiveVm> Objectives)
{
    public IEnumerable<KpiVm> AllKpis => Objectives.SelectMany(o => o.Kpis);

    public int ObjectiveCount => Objectives.Count;

    public int KpiCount => AllKpis.Count();

    /// <summary>Worst-of roll-up of the contained KPI statuses.</summary>
    public KpiStatus AggregateStatus => StatusRollup.Of(AllKpis);
}

public sealed record ObjectiveVm(
    Guid Id,
    string Name,
    string Description,
    Guid InitiativeId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    IReadOnlyList<KpiVm> Kpis)
{
    public int KpiCount => Kpis.Count;

    public KpiStatus AggregateStatus => StatusRollup.Of(Kpis);
}

public sealed record KpiVm(
    Guid Id,
    string Name,
    string Unit,
    KpiDirection Direction,
    decimal BaselineValue,
    decimal TargetValue,
    decimal CurrentValue,
    DateTime DueDate,
    KpiStatus Status,
    Guid ObjectiveId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    IReadOnlyList<MeasurementVm> Measurements)
{
    /// <summary>
    /// How far CurrentValue has travelled from BaselineValue toward TargetValue, 0–100.
    /// Direction-aware: for a Decrease KPI progress grows as the value falls toward target.
    /// </summary>
    public int ProgressPercent
    {
        get
        {
            var span = TargetValue - BaselineValue;
            if (span == 0)
            {
                return CurrentValue == TargetValue ? 100 : 0;
            }

            var fraction = (double)((CurrentValue - BaselineValue) / span);
            return (int)Math.Round(Math.Clamp(fraction, 0d, 1d) * 100);
        }
    }

    public bool IsOverdue => DueDate.Date < DateTime.Today && Status != KpiStatus.Completed;

    public string DirectionLabel => Direction == KpiDirection.Increase ? "Increase" : "Decrease";

    public string StatusLabel => Status switch
    {
        KpiStatus.NotStarted => "Not started",
        KpiStatus.OnTrack => "On track",
        KpiStatus.AtRisk => "At risk",
        KpiStatus.OffTrack => "Off track",
        KpiStatus.Completed => "Completed",
        _ => Status.ToString(),
    };

    /// <summary>CSS modifier suffix for the .pp-badge--* status pill.</summary>
    public string StatusModifier => Status switch
    {
        KpiStatus.OnTrack => "track",
        KpiStatus.AtRisk => "warn",
        KpiStatus.OffTrack => "off",
        KpiStatus.Completed => "done",
        _ => "idle",
    };
}

public sealed record MeasurementVm(
    Guid Id,
    decimal Value,
    string? Notes,
    Guid KpiId,
    DateTime CreatedDate);

public static class StatusRollup
{
    /// <summary>Worst-of roll-up: any OffTrack → OffTrack, else any AtRisk → AtRisk,
    /// else if every KPI is Completed → Completed, else OnTrack (NotStarted treated as OnTrack).</summary>
    public static KpiStatus Of(IEnumerable<KpiVm> kpis)
    {
        var list = kpis.ToList();
        if (list.Count == 0)
        {
            return KpiStatus.NotStarted;
        }

        if (list.Any(k => k.Status == KpiStatus.OffTrack))
        {
            return KpiStatus.OffTrack;
        }

        if (list.Any(k => k.Status == KpiStatus.AtRisk))
        {
            return KpiStatus.AtRisk;
        }

        return list.All(k => k.Status == KpiStatus.Completed) ? KpiStatus.Completed : KpiStatus.OnTrack;
    }

    public static string Label(KpiStatus status) => status switch
    {
        KpiStatus.NotStarted => "Not started",
        KpiStatus.OnTrack => "On track",
        KpiStatus.AtRisk => "At risk",
        KpiStatus.OffTrack => "Off track",
        KpiStatus.Completed => "Completed",
        _ => status.ToString(),
    };

    public static string Modifier(KpiStatus status) => status switch
    {
        KpiStatus.OnTrack => "track",
        KpiStatus.AtRisk => "warn",
        KpiStatus.OffTrack => "off",
        KpiStatus.Completed => "done",
        _ => "idle",
    };
}
