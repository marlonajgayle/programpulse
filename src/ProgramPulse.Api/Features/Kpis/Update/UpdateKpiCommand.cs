using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Kpis.Update;

public sealed record UpdateKpiCommand(
    Guid Id,
    string Name,
    string Unit,
    KpiDirection Direction,
    decimal TargetValue,
    DateTime DueDate,
    MeasurementFrequency? Frequency,
    string? Strategies,
    string? Activities,
    string? KeyOutputs,
    string? PerformanceMeasure);

/// <summary>
/// Updates a KPI the caller's tenant owns (verified via the parent Objective's
/// Programme tenant). Progress values (baseline/current/status) are not changed here;
/// they are managed through measurements. Returns a not-found error when the KPI does
/// not exist, belongs to another tenant, or has been soft-deleted.
/// </summary>
public sealed class UpdateKpiCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        UpdateKpiCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result.Failure(tenant.Error);

        var kpi = await _dbContext.Kpis
            .FirstOrDefaultAsync(
                k => k.Id == command.Id && k.Objective.Programme.TenantId == tenant.Value,
                cancellationToken);

        if (kpi is null)
            return Result.Failure(KpiErrors.KpiNotFound(command.Id));

        kpi.Update(command.Name, command.Unit, command.Direction, command.TargetValue, command.DueDate, command.Frequency,
            command.Strategies, command.Activities, command.KeyOutputs, command.PerformanceMeasure);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
