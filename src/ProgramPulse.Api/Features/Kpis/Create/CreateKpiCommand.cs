using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Kpis.Create;

public sealed record CreateKpiCommand(
    Guid ObjectiveId,
    string Name,
    string Unit,
    KpiDirection Direction,
    decimal BaselineValue,
    decimal TargetValue,
    decimal CurrentValue,
    DateTime DueDate,
    MeasurementFrequency? Frequency);

/// <summary>
/// Adds a new KPI to an Objective the caller's tenant owns (verified via the parent
/// Programme's tenant). The KPI is created through the Objective aggregate member
/// (<c>AddKpi</c>). Returns a not-found error when the objective does not exist,
/// belongs to another tenant, or has been soft-deleted.
/// </summary>
public sealed class CreateKpiCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<KpiResponse>> HandleAsync(
        CreateKpiCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<KpiResponse>.Failure(tenant.Error);

        var objective = await _dbContext.Objectives
            .FirstOrDefaultAsync(
                o => o.Id == command.ObjectiveId && o.Programme.TenantId == tenant.Value,
                cancellationToken);

        if (objective is null)
            return Result<KpiResponse>.Failure(
                ObjectiveErrors.ObjectiveNotFound(command.ObjectiveId));

        var kpi = objective.AddKpi(
            command.Name,
            command.Unit,
            command.Direction,
            command.BaselineValue,
            command.TargetValue,
            command.CurrentValue,
            command.DueDate,
            command.Frequency);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new KpiResponse(
            kpi.Id,
            kpi.Name,
            kpi.Unit,
            kpi.Direction,
            kpi.BaselineValue,
            kpi.TargetValue,
            kpi.CurrentValue,
            kpi.DueDate,
            kpi.Status,
            kpi.MeasurementFrequency,
            kpi.ObjectiveId,
            kpi.CreatedDate,
            kpi.LastModifiedDate);

        return Result<KpiResponse>.Created(response, $"/api/v1/kpis/{kpi.Id}");
    }
}
