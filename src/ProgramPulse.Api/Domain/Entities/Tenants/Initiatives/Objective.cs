using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

public sealed class Objective : AuditableEntity<Guid>
{
    private readonly List<Kpi> _kpis = [];

    // EF Core materialization
    private Objective() { }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid InitiativeId { get; private set; }
    public Initiative Initiative { get; private set; } = null!;

    public IReadOnlyCollection<Kpi> Kpis => _kpis.AsReadOnly();

    // Internal so objectives are only created through the Initiative aggregate root.
    internal static Objective Create(string name, string description, Guid initiativeId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        return new Objective
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            InitiativeId = initiativeId
        };
    }

    public void Update(string name, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Name = name;
        Description = description;
    }

    public Kpi AddKpi(
        string name,
        string unit,
        KpiDirection direction,
        decimal baselineValue,
        decimal targetValue,
        decimal currentValue,
        DateTime dueDate)
    {
        var kpi = Kpi.Create(name, unit, direction, baselineValue, targetValue, currentValue, dueDate, Id);
        _kpis.Add(kpi);
        return kpi;
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
