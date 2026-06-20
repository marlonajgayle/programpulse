using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Kpis.List;

public sealed record GetKpisQuery(Guid ObjectiveId);

/// <summary>
/// Returns all KPIs under an Objective the caller's tenant owns, ordered by creation
/// date. Soft-deleted rows are excluded by the global query filter.
/// </summary>
public sealed class GetKpisQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<KpiResponse>>> HandleAsync(
        GetKpisQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<IReadOnlyList<KpiResponse>>.Failure(tenant.Error);

        var kpis = await _dbContext.Kpis
            .AsNoTracking()
            .Where(k => k.ObjectiveId == query.ObjectiveId
                && k.Objective.Initiative.TenantId == tenant.Value)
            .OrderBy(k => k.CreatedDate)
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
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<KpiResponse>>.Success(kpis);
    }
}
