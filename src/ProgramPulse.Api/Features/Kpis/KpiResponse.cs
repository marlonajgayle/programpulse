using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

namespace ProgramPulse.Api.Features.Kpis;

/// <summary>
/// API representation of a KPI returned by the read endpoints.
/// </summary>
public sealed record KpiResponse(
    Guid Id,
    string Name,
    string Unit,
    KpiDirection Direction,
    decimal BaselineValue,
    decimal TargetValue,
    decimal CurrentValue,
    DateTime DueDate,
    KpiStatus Status,
    MeasurementFrequency? MeasurementFrequency,
    string? Strategies,
    string? Activities,
    string? KeyOutputs,
    string? PerformanceMeasure,
    Guid ObjectiveId,
    DateTime CreatedDate,
    DateTime? LastModifiedDate);
