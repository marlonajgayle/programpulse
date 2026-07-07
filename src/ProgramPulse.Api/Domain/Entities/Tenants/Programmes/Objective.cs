using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public sealed class Objective : AuditableEntity<Guid>
{
    // EF Core materialization
    private Objective() { }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid ProgrammeId { get; private set; }
    public Programme Programme { get; private set; } = null!;

    // An objective has exactly one KPI, created together with the objective.
    public Kpi Kpi { get; private set; } = null!;

    // Internal so objectives are only created through the Programme aggregate root.
    internal static Objective Create(
        string name,
        string description,
        Guid programmeId,
        string kpiName,
        string kpiUnit,
        KpiDirection kpiDirection,
        decimal baselineValue,
        decimal targetValue,
        decimal currentValue,
        DateTime dueDate,
        MeasurementFrequency? kpiFrequency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var id = Guid.CreateVersion7();

        return new Objective
        {
            Id = id,
            Name = name,
            Description = description,
            ProgrammeId = programmeId,
            Kpi = Kpi.Create(
                kpiName,
                kpiUnit,
                kpiDirection,
                baselineValue,
                targetValue,
                currentValue,
                dueDate,
                kpiFrequency,
                id)
        };
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
