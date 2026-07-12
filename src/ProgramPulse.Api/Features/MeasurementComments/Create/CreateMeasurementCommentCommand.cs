using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.MeasurementComments.Create;

public sealed record CreateMeasurementCommentCommand(Guid MeasurementId, string Text);

/// <summary>
/// Adds a comment to a Measurement the caller's tenant owns (verified via the KPI →
/// Objective → Programme tenant chain). Returns a not-found error when the Measurement
/// does not exist, belongs to another tenant, or has been soft-deleted.
/// </summary>
public sealed class CreateMeasurementCommentCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<MeasurementCommentResponse>> HandleAsync(
        CreateMeasurementCommentCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<MeasurementCommentResponse>.Failure(tenant.Error);

        var measurement = await _dbContext.Measurements
            .FirstOrDefaultAsync(
                m => m.Id == command.MeasurementId
                    && m.Kpi.Objective.Programme.TenantId == tenant.Value,
                cancellationToken);

        if (measurement is null)
            return Result<MeasurementCommentResponse>.Failure(
                MeasurementErrors.MeasurementNotFound(command.MeasurementId));

        var comment = measurement.AddComment(command.Text);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new MeasurementCommentResponse(
            comment.Id,
            comment.Text,
            comment.MeasurementId,
            comment.CreatedBy,
            comment.CreatedDate,
            comment.LastModifiedDate);

        return Result<MeasurementCommentResponse>.Created(response, $"/api/v1/comments/{comment.Id}");
    }
}
