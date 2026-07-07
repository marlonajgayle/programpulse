using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Programmes.List;

public sealed record GetProgrammesQuery(int Page = 1, int PageSize = 10);

/// <summary>
/// Returns a page of top-level Programmes within the caller's tenant, ordered by
/// creation date, each with its sub-programmes (one level of nesting) nested under it.
/// Only top-level programmes count toward pagination; sub-programmes ride along inside
/// their parent. Soft-deleted Programmes are excluded by the global query filter.
/// </summary>
public sealed class GetProgrammesQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private const int MaxPageSize = 200;

    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<PagedList<ProgrammeResponse>>> HandleAsync(
        GetProgrammesQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<PagedList<ProgrammeResponse>>.Failure(tenant.Error);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var parentsQuery = _dbContext.Programmes
            .AsNoTracking()
            .Where(i => i.TenantId == tenant.Value && i.ParentProgrammeId == null);

        var totalCount = await parentsQuery.CountAsync(cancellationToken);

        var parents = await parentsQuery
            .OrderBy(i => i.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToResponse)
            .ToListAsync(cancellationToken);

        var parentIds = parents.Select(p => p.Id).ToList();

        var children = await _dbContext.Programmes
            .AsNoTracking()
            .Where(i => i.TenantId == tenant.Value
                && i.ParentProgrammeId != null
                && parentIds.Contains(i.ParentProgrammeId.Value))
            .OrderBy(i => i.CreatedDate)
            .Select(ToResponse)
            .ToListAsync(cancellationToken);

        var childrenByParent = children.ToLookup(c => c.ParentProgrammeId);

        var items = parents
            .Select(p => p with { SubProgrammes = childrenByParent[p.Id].ToList() })
            .ToList();

        var result = new PagedList<ProgrammeResponse>(items, page, pageSize, totalCount);
        return Result<PagedList<ProgrammeResponse>>.Success(result);
    }

    // Shared projection for both parents and children. SubProgrammes is left null here
    // and attached to parents afterwards; children keep it null (rendered as empty).
    private static readonly Expression<Func<Programme, ProgrammeResponse>> ToResponse =
        i => new ProgrammeResponse(
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
            i.Objectives.Count(o => o.Kpi != null),
            i.ParentProgrammeId,
            null);
}
