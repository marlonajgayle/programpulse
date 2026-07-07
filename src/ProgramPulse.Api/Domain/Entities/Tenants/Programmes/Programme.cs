using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public sealed class Programme : AuditableEntity<Guid>
{
    private readonly List<Objective> _objectives = [];

    // EF Core materialization
    private Programme() { }

    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime? StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public Guid TenantId { get; private set; }
    public Tenant Tenant { get; private set; } = null!;

    // Optional link to a parent programme so programmes can nest.
    public Guid? ParentProgrammeId { get; private set; }
    public Programme? ParentProgramme { get; private set; }

    /// <summary>
    /// Lifecycle status derived from <see cref="EndDate"/>: a programme is
    /// <see cref="ProgrammeStatus.Active"/> while it has no end date or its end
    /// date is still in the future, and <see cref="ProgrammeStatus.Archived"/>
    /// once the end date has passed. Not persisted.
    /// </summary>
    public ProgrammeStatus Status =>
        EndDate is null || DateTime.UtcNow < EndDate
            ? ProgrammeStatus.Active
            : ProgrammeStatus.Archived;

    public IReadOnlyCollection<Objective> Objectives => _objectives.AsReadOnly();

    public static Programme Create(
        string name,
        string description,
        DateTime? startDate,
        DateTime? endDate,
        Guid tenantId,
        Guid? parentProgrammeId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ValidateDateRange(startDate, endDate);

        return new Programme
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Description = description,
            StartDate = startDate,
            EndDate = endDate,
            TenantId = tenantId,
            ParentProgrammeId = parentProgrammeId
        };
    }

    public void Update(
        string name,
        string description,
        DateTime? startDate,
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

    /// <summary>
    /// Sets or clears the parent programme. Pass <c>null</c> to promote this programme
    /// back to top-level. Guards against a programme parenting itself; callers are
    /// responsible for validating the parent exists, is in-tenant, and is top-level.
    /// </summary>
    public void SetParent(Guid? parentProgrammeId)
    {
        if (parentProgrammeId == Id)
        {
            throw new ArgumentException("A programme cannot be its own parent.", nameof(parentProgrammeId));
        }

        ParentProgrammeId = parentProgrammeId;
    }

    public Objective AddObjective(
        string name,
        string description,
        string kpiName,
        string kpiUnit,
        KpiDirection kpiDirection,
        decimal baselineValue,
        decimal targetValue,
        decimal currentValue,
        DateTime dueDate,
        MeasurementFrequency? kpiFrequency)
    {
        var objective = Objective.Create(
            name,
            description,
            Id,
            kpiName,
            kpiUnit,
            kpiDirection,
            baselineValue,
            targetValue,
            currentValue,
            dueDate,
            kpiFrequency);
        _objectives.Add(objective);
        return objective;
    }

    private static void ValidateDateRange(DateTime? startDate, DateTime? endDate)
    {
        if (startDate is not null && endDate is not null && endDate < startDate)
        {
            throw new ArgumentException("End date cannot be earlier than start date.", nameof(endDate));
        }
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
