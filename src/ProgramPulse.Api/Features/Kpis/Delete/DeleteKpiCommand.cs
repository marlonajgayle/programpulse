using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Kpis.Delete;

public sealed record DeleteKpiCommand(Guid Id);

/// <summary>
/// Soft-deletes a KPI the caller's tenant owns (verified via the parent Objective's
/// Initiative tenant).
/// </summary>
public sealed class DeleteKpiCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        DeleteKpiCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result.Failure(tenant.Error);

        var kpi = await _dbContext.Kpis
            .FirstOrDefaultAsync(
                k => k.Id == command.Id && k.Objective.Initiative.TenantId == tenant.Value,
                cancellationToken);

        if (kpi is null)
            return Result.Failure(KpiErrors.KpiNotFound(command.Id));

        kpi.MarkAsDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
