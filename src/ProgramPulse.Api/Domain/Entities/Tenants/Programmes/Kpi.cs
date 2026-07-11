using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public sealed class Kpi : AuditableEntity<Guid>
{
    private readonly List<Measurement> _measurements = [];

    // EF Core materialization
    private Kpi() { }

    public string Name { get; private set; } = string.Empty;
    public string Unit { get; private set; } = string.Empty;
    public KpiDirection Direction { get; private set; }
    public decimal BaselineValue { get; private set; }
    public decimal TargetValue { get; private set; }
    public decimal CurrentValue { get; private set; }
    public DateTime DueDate { get; private set; }
    public KpiStatus Status { get; private set; }

    // Expected measurement cadence. When null there is no cadence limit and
    // measurements may be recorded without restriction.
    public MeasurementFrequency? MeasurementFrequency { get; private set; }

    // Optional results-framework planning notes. Free-text, may be null.
    public string? Strategies { get; private set; }
    public string? Activities { get; private set; }
    public string? KeyOutputs { get; private set; }
    public string? PerformanceMeasure { get; private set; }

    public Guid ObjectiveId { get; private set; }
    public Objective Objective { get; private set; } = null!;

    public IReadOnlyCollection<Measurement> Measurements => _measurements.AsReadOnly();

    // Internal so KPIs are only created through the Objective.
    internal static Kpi Create(
        string name,
        string unit,
        KpiDirection direction,
        decimal baselineValue,
        decimal targetValue,
        decimal currentValue,
        DateTime dueDate,
        MeasurementFrequency? measurementFrequency,
        string? strategies,
        string? activities,
        string? keyOutputs,
        string? performanceMeasure,
        Guid objectiveId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);

        return new Kpi
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Unit = unit,
            Direction = direction,
            BaselineValue = baselineValue,
            TargetValue = targetValue,
            CurrentValue = currentValue,
            DueDate = dueDate,
            Status = KpiStatus.NotStarted,
            MeasurementFrequency = measurementFrequency,
            Strategies = strategies,
            Activities = activities,
            KeyOutputs = keyOutputs,
            PerformanceMeasure = performanceMeasure,
            ObjectiveId = objectiveId
        };
    }

    public void Update(
        string name,
        string unit,
        KpiDirection direction,
        decimal targetValue,
        DateTime dueDate,
        MeasurementFrequency? measurementFrequency,
        string? strategies,
        string? activities,
        string? keyOutputs,
        string? performanceMeasure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(unit);

        Name = name;
        Unit = unit;
        Direction = direction;
        TargetValue = targetValue;
        DueDate = dueDate;
        MeasurementFrequency = measurementFrequency;
        Strategies = strategies;
        Activities = activities;
        KeyOutputs = keyOutputs;
        PerformanceMeasure = performanceMeasure;
    }

    public void RecordProgress(decimal currentValue, KpiStatus status)
    {
        CurrentValue = currentValue;
        Status = status;
    }

    public Measurement AddMeasurement(decimal value, string? notes, DateTime measurementDate)
    {
        var measurement = Measurement.Create(value, notes, measurementDate, Id);
        _measurements.Add(measurement);
        CurrentValue = value; // keep current reading in sync with latest measurement
        return measurement;
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
