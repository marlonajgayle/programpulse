using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Initiatives.GetById;

public sealed record GetInitiativeByIdQuery(Guid Id);

/// <summary>
/// Returns a single Initiative the caller's tenant owns, with its objectives and each
/// objective's KPIs. Returns a not-found error when the Initiative does not exist,
/// belongs to another tenant, or has been soft-deleted (excluded by the global filter).
/// </summary>
public sealed class GetInitiativeByIdQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<InitiativeDetailResponse>> HandleAsync(
        GetInitiativeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<InitiativeDetailResponse>.Failure(tenant.Error);

        var detail = await _dbContext.Initiatives
            .AsNoTracking()
            .Where(i => i.Id == query.Id && i.TenantId == tenant.Value)
            .Select(i => new InitiativeDetailResponse(
                i.Id,
                i.Name,
                i.Description,
                i.StartDate,
                i.EndDate,
                i.CreatedDate,
                i.LastModifiedDate,
                i.Objectives
                    .OrderBy(o => o.CreatedDate)
                    .Select(o => new ObjectiveDetailResponse(
                        o.Id,
                        o.Name,
                        o.Description,
                        o.InitiativeId,
                        o.CreatedDate,
                        o.LastModifiedDate,
                        o.Kpis
                            .OrderBy(k => k.CreatedDate)
                            .Select(k => new KpiDetailResponse(
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
                            .ToList()))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (detail is null)
            return Result<InitiativeDetailResponse>.Failure(
                InitiativeErrors.InitiativeNotFound(query.Id));

        return Result<InitiativeDetailResponse>.Success(detail);
    }
}
