using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Objectives.List;

public sealed record GetObjectivesQuery(Guid InitiativeId);

/// <summary>
/// Returns all Objectives under an Initiative the caller's tenant owns, ordered by
/// creation date. Soft-deleted rows are excluded by the global query filter.
/// </summary>
public sealed class GetObjectivesQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<ObjectiveResponse>>> HandleAsync(
        GetObjectivesQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<IReadOnlyList<ObjectiveResponse>>.Failure(tenant.Error);

        var objectives = await _dbContext.Objectives
            .AsNoTracking()
            .Where(o => o.InitiativeId == query.InitiativeId
                && o.Initiative.TenantId == tenant.Value)
            .OrderBy(o => o.CreatedDate)
            .Select(o => new ObjectiveResponse(
                o.Id,
                o.Name,
                o.Description,
                o.InitiativeId,
                o.CreatedDate,
                o.LastModifiedDate))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ObjectiveResponse>>.Success(objectives);
    }
}
