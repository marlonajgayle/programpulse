using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

namespace ProgramPulse.Api.Features.Programmes;

/// <summary>
/// API representation of a Programme returned by the read endpoints.
/// </summary>
public sealed record ProgrammeResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime? StartDate,
    DateTime? EndDate,
    ProgrammeStatus Status,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    int ObjectiveCount,
    int KpiCount);
