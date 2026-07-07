using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Kpis.List;

public sealed record GetObjectiveKpiQuery(Guid ObjectiveId);

/// <summary>
/// Returns the single KPI belonging to an Objective the caller's tenant owns. An
/// objective always has exactly one KPI; a not-found error is returned when the objective
/// does not exist or belongs to another tenant. Soft-deleted rows are excluded by the
/// global query filter.
/// </summary>
public sealed class GetObjectiveKpiQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<KpiResponse>> HandleAsync(
        GetObjectiveKpiQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<KpiResponse>.Failure(tenant.Error);

        var kpi = await _dbContext.Kpis
            .AsNoTracking()
            .Where(k => k.ObjectiveId == query.ObjectiveId
                && k.Objective.Programme.TenantId == tenant.Value)
            .Select(k => new KpiResponse(
                k.Id,
                k.Name,
                k.Unit,
                k.Direction,
                k.BaselineValue,
                k.TargetValue,
                k.CurrentValue,
                k.DueDate,
                k.Status,
                k.ObjectiveId,
                k.CreatedDate,
                k.LastModifiedDate))
            .FirstOrDefaultAsync(cancellationToken);

        if (kpi is null)
            return Result<KpiResponse>.Failure(
                ObjectiveErrors.ObjectiveNotFound(query.ObjectiveId));

        return Result<KpiResponse>.Success(kpi);
    }
}
