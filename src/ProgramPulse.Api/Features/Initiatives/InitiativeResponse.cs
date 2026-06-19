namespace ProgramPulse.Api.Features.Initiatives;

/// <summary>
/// API representation of an Initiative returned by the read endpoints.
/// </summary>
public sealed record InitiativeResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
