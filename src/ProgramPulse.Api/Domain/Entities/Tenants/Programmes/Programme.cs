using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

public sealed class Initiative : AuditableEntity<Guid>
{
    private readonly List<Objective> _objectives = [];

    // EF Core materialization
    private Initiative() { }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    public IReadOnlyCollection<Objective> Objectives => _objectives.AsReadOnly();

    public static Initiative Create(
        string name,
        string description,
        DateTime startDate,
        DateTime? endDate,
        Guid tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ValidateDateRange(startDate, endDate);

        return new Initiative
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            TenantId = tenantId
        };
    }

    public void Update(
        string name,
        string description,
        DateTime startDate,
        DateTime? endDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ValidateDateRange(startDate, endDate);

        Name = name;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
    }

    public Objective AddObjective(string name, string description)
    {
        var objective = Objective.Create(name, description, Id);
        _objectives.Add(objective);
        return objective;
    }

    private static void ValidateDateRange(DateTime startDate, DateTime? endDate)
    {
        if (endDate is not null && endDate < startDate)
        {
            throw new ArgumentException("End date cannot be earlier than start date.", nameof(endDate));
        }
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
