using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Measurements.List;

public sealed record GetMeasurementsQuery(Guid KpiId);

/// <summary>
/// Returns all Measurements for a KPI the caller's tenant owns, ordered by creation
/// date. Soft-deleted rows are excluded by the global query filter.
/// </summary>
public sealed class GetMeasurementsQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<MeasurementResponse>>> HandleAsync(
        GetMeasurementsQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<IReadOnlyList<MeasurementResponse>>.Failure(tenant.Error);

        var measurements = await _dbContext.Measurements
            .AsNoTracking()
            .Where(m => m.KpiId == query.KpiId
                && m.Kpi.Objective.Initiative.TenantId == tenant.Value)
            .OrderBy(m => m.CreatedDate)
            .Select(m => new MeasurementResponse(
                m.Id,
                m.Value,
                m.Notes,
                m.KpiId,
                m.CreatedDate,
                m.LastModifiedDate))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MeasurementResponse>>.Success(measurements);
    }
}
