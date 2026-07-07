namespace ProgramPulse.Api.Features.Objectives;

/// <summary>
/// API representation of an Objective returned by the read endpoints.
/// </summary>
public sealed record ObjectiveResponse(
    Guid Id,
    string Name,
    string Description,
    Guid ProgrammeId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
