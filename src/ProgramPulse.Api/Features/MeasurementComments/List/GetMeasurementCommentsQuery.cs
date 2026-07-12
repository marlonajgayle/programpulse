using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.MeasurementComments.List;

public sealed record GetMeasurementCommentsQuery(Guid MeasurementId);

/// <summary>
/// Returns all comments for a Measurement the caller's tenant owns, ordered oldest-first.
/// Soft-deleted rows are excluded by the global query filter.
/// </summary>
public sealed class GetMeasurementCommentsQueryHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyList<MeasurementCommentResponse>>> HandleAsync(
        GetMeasurementCommentsQuery query,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<IReadOnlyList<MeasurementCommentResponse>>.Failure(tenant.Error);

        var comments = await _dbContext.MeasurementComments
            .AsNoTracking()
            .Where(c => c.MeasurementId == query.MeasurementId
                && c.Measurement.Kpi.Objective.Programme.TenantId == tenant.Value)
            .OrderBy(c => c.CreatedDate)
            .Select(c => new MeasurementCommentResponse(
                c.Id,
                c.Text,
                c.MeasurementId,
                c.CreatedBy,
                c.CreatedDate,
                c.LastModifiedDate))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<MeasurementCommentResponse>>.Success(comments);
    }
}
