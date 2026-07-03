using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Initiatives.List;

public sealed record GetInitiativesQuery;

/// <summary>
/// Returns all Initiatives within the caller's tenant, ordered by creation date.
/// Soft-deleted Initiatives are excluded by the global query filter.
/// </summary>
public sealed class GetInitiativesQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<InitiativeResponse>>> HandleAsync(
        GetInitiativesQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<IReadOnlyList<InitiativeResponse>>.Failure(tenant.Error);

        var initiatives = await _dbContext.Initiatives
            .AsNoTracking()
            .Where(i => i.TenantId == tenant.Value)
            .OrderBy(i => i.CreatedDate)
            .Select(i => new InitiativeResponse(
                i.Id,
                i.Name,
                i.Description,
                i.StartDate,
                i.EndDate,
                i.CreatedDate,
                i.LastModifiedDate,
                i.Objectives.Count,
                i.Objectives.SelectMany(o => o.Kpis).Count()))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<InitiativeResponse>>.Success(initiatives);
    }
}
