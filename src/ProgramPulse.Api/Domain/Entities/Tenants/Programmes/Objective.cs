using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public sealed class Objective : AuditableEntity<Guid>
{
    private readonly List<Kpi> _kpis = [];

    // EF Core materialization
    private Objective() { }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid ProgrammeId { get; private set; }
    public Programme Programme { get; private set; } = null!;

    // An objective can have many KPIs, added through AddKpi.
    public IReadOnlyCollection<Kpi> Kpis => _kpis.AsReadOnly();

    // Internal so objectives are only created through the Programme aggregate root.
    internal static Objective Create(
        string name,
        string description,
        Guid programmeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Objective
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            ProgrammeId = programmeId
        };
    }

    public Kpi AddKpi(
        string kpiName,
        string kpiUnit,
        KpiDirection kpiDirection,
        decimal baselineValue,
        decimal targetValue,
        decimal currentValue,
        DateTime dueDate,
        MeasurementFrequency? kpiFrequency,
        string? strategies,
        string? activities,
        string? keyOutputs,
        string? performanceMeasure)
    {
        var kpi = Kpi.Create(
            kpiName,
            kpiUnit,
            kpiDirection,
            baselineValue,
            targetValue,
            currentValue,
            dueDate,
            kpiFrequency,
            strategies,
            activities,
            keyOutputs,
            performanceMeasure,
            Id);
        _kpis.Add(kpi);
        return kpi;
    }

    public void Update(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Name = name;
        Description = description;
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
