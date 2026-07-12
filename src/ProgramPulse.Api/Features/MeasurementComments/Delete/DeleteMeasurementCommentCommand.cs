using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.MeasurementComments.Delete;

public sealed record DeleteMeasurementCommentCommand(Guid Id);

/// <summary>
/// Soft-deletes a measurement comment the caller's tenant owns (verified via the
/// Measurement → KPI → Objective → Programme tenant chain).
/// </summary>
public sealed class DeleteMeasurementCommentCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        DeleteMeasurementCommentCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result.Failure(tenant.Error);

        var comment = await _dbContext.MeasurementComments
            .FirstOrDefaultAsync(
                c => c.Id == command.Id
                    && c.Measurement.Kpi.Objective.Programme.TenantId == tenant.Value,
                cancellationToken);

        if (comment is null)
            return Result.Failure(MeasurementCommentErrors.CommentNotFound(command.Id));

        comment.MarkAsDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
