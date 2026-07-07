using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

public sealed class Measurement : AuditableEntity<Guid>
{
    // EF Core materialization
    private Measurement() { }

    public decimal Value { get; private set; }
    public string? Notes { get; private set; }
    public Guid KpiId { get; private set; }
    public Kpi Kpi { get; private set; } = null!;

    // Internal so measurements are only created through the Kpi.
    internal static Measurement Create(decimal value, string? notes, Guid kpiId)
    {
        return new Measurement
        {
            Id = Guid.CreateVersion7(),
            Value = value,
            Notes = notes,
            KpiId = kpiId
        };
    }

    public void Update(decimal value, string? notes)
    {
        Value = value;
        Notes = notes;
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
