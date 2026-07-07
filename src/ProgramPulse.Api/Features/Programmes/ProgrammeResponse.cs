using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

namespace ProgramPulse.Api.Features.Programmes;

/// <summary>
/// API representation of a Programme returned by the read endpoints.
/// <see cref="ParentProgrammeId"/> is set on sub-programmes; <see cref="SubProgrammes"/>
/// is populated on top-level programmes (one level of nesting) and empty otherwise.
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
    int KpiCount,
    Guid? ParentProgrammeId = null,
    IReadOnlyList<ProgrammeResponse>? SubProgrammes = null);
