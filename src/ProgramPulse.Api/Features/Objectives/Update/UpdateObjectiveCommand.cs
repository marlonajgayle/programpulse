using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Objectives.Update;

public sealed record UpdateObjectiveCommand(Guid Id, string Name, string Description);

/// <summary>
/// Updates an Objective the caller's tenant owns (verified via the parent Initiative's
/// tenant). Returns a not-found error when the Objective does not exist, belongs to
/// another tenant, or has been soft-deleted.
/// </summary>
public sealed class UpdateObjectiveCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        UpdateObjectiveCommand command,
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

        objective.Update(command.Name, command.Description);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
