namespace ProgramPulse.Api.Domain.Entities.Tenants;

using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.SharedKernel;

public sealed class Tenant : AuditableEntity<Guid>
{
    private readonly List<ApplicationUser> _users = [];

    // EF Core materialization
    private Tenant() { }

    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    public IReadOnlyCollection<ApplicationUser> Users => _users.AsReadOnly();

    public static Tenant Create(string name, string slug, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Tenant
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Slug = slug,
            Description = description
        };
    }

    public void Update(string name, string slug, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        Name = name;
        Slug = slug;
        Description = description;
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
