using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Programmes.List;

public sealed record GetProgrammesQuery;

/// <summary>
/// Returns all Programmes within the caller's tenant, ordered by creation date.
/// Soft-deleted Programmes are excluded by the global query filter.
/// </summary>
public sealed class GetProgrammesQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<ProgrammeResponse>>> HandleAsync(
        GetProgrammesQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<IReadOnlyList<ProgrammeResponse>>.Failure(tenant.Error);

        var programmes = await _dbContext.Programmes
            .AsNoTracking()
            .Where(i => i.TenantId == tenant.Value)
            .OrderBy(i => i.CreatedDate)
            .Select(i => new ProgrammeResponse(
                i.Id,
                i.Name,
                i.Description,
                i.StartDate,
                i.EndDate,
                i.EndDate == null || DateTime.UtcNow < i.EndDate
                    ? ProgrammeStatus.Active
                    : ProgrammeStatus.Archived,
                i.CreatedDate,
                i.LastModifiedDate,
                i.Objectives.Count,
                i.Objectives.SelectMany(o => o.Kpis).Count()))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<ProgrammeResponse>>.Success(programmes);
    }
}
