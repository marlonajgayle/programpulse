namespace ProgramPulse.Api.Features.MeasurementComments;

/// <summary>
/// API representation of a comment on a KPI measurement returned by the read endpoints.
/// <c>CreatedBy</c> is the author (captured from the audit fields).
/// </summary>
public sealed record MeasurementCommentResponse(
    Guid Id,
    string Text,
    Guid MeasurementId,
    string CreatedBy,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
