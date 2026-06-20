using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Objectives.Delete;

public sealed record DeleteObjectiveCommand(Guid Id);

/// <summary>
/// Soft-deletes an Objective the caller's tenant owns (verified via the parent
/// Initiative's tenant).
/// </summary>
public sealed class DeleteObjectiveCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        DeleteObjectiveCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result.Failure(tenant.Error);

        var objective = await _dbContext.Objectives
            .FirstOrDefaultAsync(
                o => o.Id == command.Id && o.Initiative.TenantId == tenant.Value,
                cancellationToken);

        if (objective is null)
            return Result.Failure(ObjectiveErrors.ObjectiveNotFound(command.Id));

        objective.MarkAsDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
