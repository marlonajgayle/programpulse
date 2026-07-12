using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public sealed class MeasurementComment : AuditableEntity<Guid>
{
    // EF Core materialization
    private MeasurementComment() { }

    public string Text { get; private set; } = string.Empty;

    public Guid MeasurementId { get; private set; }
    public Measurement Measurement { get; private set; } = null!;

    // Internal so comments are only created through the Measurement.
    internal static MeasurementComment Create(string text, Guid measurementId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        return new MeasurementComment
        {
            Id = Guid.CreateVersion7(),
            Text = text,
            MeasurementId = measurementId
        };
    }

    public void Update(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        Text = text;
    }

    // MarkAsDeleted() is inherited (public) from BaseEntity<Guid>.
}
