using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Programmes.GetById;

public sealed record GetProgrammeByIdQuery(Guid Id);

/// <summary>
/// Returns a single Programme the caller's tenant owns, with its objectives and each
/// objective's KPIs. Returns a not-found error when the Programme does not exist,
/// belongs to another tenant, or has been soft-deleted (excluded by the global filter).
/// </summary>
public sealed class GetProgrammeByIdQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<ProgrammeDetailResponse>> HandleAsync(
        GetProgrammeByIdQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<ProgrammeDetailResponse>.Failure(tenant.Error);

        var detail = await _dbContext.Programmes
            .AsNoTracking()
            .Where(i => i.Id == query.Id && i.TenantId == tenant.Value)
            .Select(i => new ProgrammeDetailResponse(
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
                i.Objectives
                    .OrderBy(o => o.CreatedDate)
                    .Select(o => new ObjectiveDetailResponse(
                        o.Id,
                        o.Name,
                        o.Description,
                        o.ProgrammeId,
                        o.CreatedDate,
                        o.LastModifiedDate,
                        new KpiDetailResponse(
                            o.Kpi.Id,
                            o.Kpi.Name,
                            o.Kpi.Unit,
                            o.Kpi.Direction,
                            o.Kpi.BaselineValue,
                            o.Kpi.TargetValue,
                            o.Kpi.CurrentValue,
                            o.Kpi.DueDate,
                            o.Kpi.Status,
                            o.Kpi.ObjectiveId,
                            o.Kpi.CreatedDate,
                            o.Kpi.LastModifiedDate)))
                    .ToList()))
            .FirstOrDefaultAsync(cancellationToken);

        if (detail is null)
            return Result<ProgrammeDetailResponse>.Failure(
                ProgrammeErrors.ProgrammeNotFound(query.Id));

        return Result<ProgrammeDetailResponse>.Success(detail);
    }
}
