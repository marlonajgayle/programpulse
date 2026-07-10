using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Kpis.List;

public sealed record GetObjectiveKpisQuery(Guid ObjectiveId);

/// <summary>
/// Returns the KPIs belonging to an Objective the caller's tenant owns, ordered by creation
/// date. A not-found error is returned when the objective does not exist or belongs to another
/// tenant; an existing objective with no KPIs returns an empty list. Soft-deleted rows are
/// excluded by the global query filter.
/// </summary>
public sealed class GetObjectiveKpisQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<KpiResponse>>> HandleAsync(
        GetObjectiveKpisQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<IReadOnlyList<KpiResponse>>.Failure(tenant.Error);

        var objectiveExists = await _dbContext.Objectives
            .AsNoTracking()
            .AnyAsync(
                o => o.Id == query.ObjectiveId && o.Programme.TenantId == tenant.Value,
                cancellationToken);

        if (!objectiveExists)
            return Result<IReadOnlyList<KpiResponse>>.Failure(
                ObjectiveErrors.ObjectiveNotFound(query.ObjectiveId));

        var kpis = await _dbContext.Kpis
            .AsNoTracking()
            .Where(k => k.ObjectiveId == query.ObjectiveId)
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
                k.MeasurementFrequency,
                k.ObjectiveId,
                k.CreatedDate,
                k.LastModifiedDate))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<KpiResponse>>.Success(kpis);
    }
}
