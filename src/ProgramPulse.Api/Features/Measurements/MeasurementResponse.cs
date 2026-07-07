namespace ProgramPulse.Api.Features.Measurements;

/// <summary>
/// API representation of a Measurement returned by the read endpoints.
/// </summary>
public sealed record MeasurementResponse(
    Guid Id,
    decimal Value,
    string? Notes,
    DateTime MeasurementDate,
    Guid KpiId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
