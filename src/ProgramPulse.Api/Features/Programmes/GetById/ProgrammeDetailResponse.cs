using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

namespace ProgramPulse.Api.Features.Initiatives.GetById;

/// <summary>
/// API representation of a single Initiative with its full objectives → KPIs sub-tree,
/// returned by <c>GET api/v1/initiatives/{id}</c>. Measurements are not included.
/// </summary>
public sealed record InitiativeDetailResponse(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime? EndDate,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    IReadOnlyList<ObjectiveDetailResponse> Objectives);

/// <summary>An Objective nested within an <see cref="InitiativeDetailResponse"/>, with its KPIs.</summary>
public sealed record ObjectiveDetailResponse(
    Guid Id,
    string Name,
    string Description,
    Guid InitiativeId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate,
    IReadOnlyList<KpiDetailResponse> Kpis);

/// <summary>A KPI nested within an <see cref="ObjectiveDetailResponse"/>. Mirrors <c>KpiResponse</c>.</summary>
public sealed record KpiDetailResponse(
    Guid Id,
    string Name,
    string Unit,
    KpiDirection Direction,
    decimal BaselineValue,
    decimal TargetValue,
    decimal CurrentValue,
    DateTime DueDate,
    KpiStatus Status,
    Guid ObjectiveId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
