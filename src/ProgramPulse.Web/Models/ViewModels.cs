namespace ProgramPulse.Web.Models;

// View models mirroring the API response DTOs (ProgrammeResponse, ObjectiveResponse,
// KpiResponse, MeasurementResponse) but carrying their child collections so a page can
// render the whole sub-tree from mock data. Swap SampleData for an API-backed source later.

public sealed record ProgrammeVm(
    Guid Id,
    string Name,
    string Description,
    DateTime? StartDate,
    DateTime? EndDate,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    IReadOnlyList<ObjectiveVm> Objectives,
    int? ObjectiveCountOverride = null,
    int? KpiCountOverride = null)
{
    public IEnumerable<KpiVm> AllKpis => Objectives.SelectMany(o => o.Kpis);

    public int ObjectiveCount => ObjectiveCountOverride ?? Objectives.Count;

    public int KpiCount => KpiCountOverride ?? AllKpis.Count();

    /// <summary>Worst-of roll-up of the contained KPI statuses.</summary>
    public KpiStatus AggregateStatus => StatusRollup.Of(AllKpis);

    /// <summary>Lifecycle status derived from <see cref="EndDate"/>, mirroring the API:
    /// Active while there is no end date or it is still in the future, else Archived.
    /// Distinct from <see cref="AggregateStatus"/> (the KPI roll-up).</summary>
    public ProgrammeStatus LifecycleStatus =>
        EndDate is null || DateTime.UtcNow < EndDate
            ? ProgrammeStatus.Active
            : ProgrammeStatus.Archived;

    public string LifecycleStatusLabel => LifecycleStatus == ProgrammeStatus.Active ? "Active" : "Archived";

    /// <summary>Average progress across the contained KPIs, 0–100 (0 when there are none).</summary>
    public int ProgressPercent
    {
        get
        {
            var kpis = AllKpis.ToList();
            return kpis.Count == 0 ? 0 : (int)Math.Round(kpis.Average(k => k.ProgressPercent));
        }
    }
}

public sealed record ObjectiveVm(
    Guid Id,
    string Name,
    string Description,
    Guid ProgrammeId,
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

    /// <summary>Timestamp of the most recent measurement, or null when none have been
    /// logged. Derived from <see cref="Measurements"/> so it needs no separate API field.</summary>
    public DateTime? LastMeasuredAt =>
        Measurements.Count == 0 ? null : Measurements.Max(m => m.CreatedDate);

    public int MeasurementCount => Measurements.Count;

    /// <summary>Whole days since the last measurement, or null when none have been logged.</summary>
    public int? DaysSinceLastMeasured =>
        LastMeasuredAt is null ? null : (int)(DateTime.Today - LastMeasuredAt.Value.Date).TotalDays;

    /// <summary>True when a reading is overdue: either none has ever been logged, or the
    /// latest is older than <see cref="KpiThresholds.OverdueMeasurementDays"/>.</summary>
    public bool IsMeasurementOverdue =>
        DaysSinceLastMeasured is null || DaysSinceLastMeasured > KpiThresholds.OverdueMeasurementDays;

    /// <summary>The most recent measurement values, oldest → newest, capped for the sparkline.</summary>
    public IReadOnlyList<decimal> RecentMeasurementValues =>
        Measurements
            .OrderBy(m => m.CreatedDate)
            .Select(m => m.Value)
            .TakeLast(KpiThresholds.SparklinePoints)
            .ToList();

    /// <summary>Due-date urgency: neutral until the due date is within
    /// <see cref="KpiThresholds.DueSoonDays"/>, warn inside that window, danger once passed.</summary>
    public KpiDueUrgency DueUrgency
    {
        get
        {
            if (IsOverdue)
            {
                return KpiDueUrgency.Overdue;
            }

            var daysUntilDue = (DueDate.Date - DateTime.Today).TotalDays;
            return daysUntilDue <= KpiThresholds.DueSoonDays ? KpiDueUrgency.DueSoon : KpiDueUrgency.OnSchedule;
        }
    }

    /// <summary>CSS modifier for the due-date chip colour (empty = neutral).</summary>
    public string DueChipModifier => DueUrgency switch
    {
        KpiDueUrgency.Overdue => "off",
        KpiDueUrgency.DueSoon => "warn",
        _ => string.Empty,
    };

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
