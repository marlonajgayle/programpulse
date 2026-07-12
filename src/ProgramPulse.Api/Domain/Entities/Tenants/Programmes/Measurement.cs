using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public sealed class Measurement : AuditableEntity<Guid>
{
    private readonly List<MeasurementComment> _comments = [];

    // EF Core materialization
    private Measurement() { }

    public decimal Value { get; private set; }
    public string? Notes { get; private set; }

    // The date the reading is for. May be back-dated to record historical entries;
    // it (not the audit CreatedDate) is what the KPI's cadence rule measures against.
    public DateTime MeasurementDate { get; private set; }

    public Guid KpiId { get; private set; }
    public Kpi Kpi { get; private set; } = null!;

    // Free-text comments/questions about the measured value.
    public IReadOnlyCollection<MeasurementComment> Comments => _comments.AsReadOnly();

    // Internal so measurements are only created through the Kpi.
    internal static Measurement Create(decimal value, string? notes, DateTime measurementDate, Guid kpiId)
    {
        return new Measurement
        {
            Id = Guid.CreateVersion7(),
            Value = value,
            Notes = notes,
            MeasurementDate = measurementDate,
            KpiId = kpiId
        };
    }

    public void Update(decimal value, string? notes, DateTime measurementDate)
    {
        Value = value;
        Notes = notes;
        MeasurementDate = measurementDate;
    }

    // Internal callers add comments through this aggregate method so comments are
    // only ever created via the owning Measurement.
    public MeasurementComment AddComment(string text)
    {
        var comment = MeasurementComment.Create(text, Id);
        _comments.Add(comment);
        return comment;
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
