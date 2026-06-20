using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Objectives.Create;

public sealed record CreateObjectiveCommand(
    Guid InitiativeId,
    string Name,
    string Description);

/// <summary>
/// Creates a new Objective under an Initiative the caller's tenant owns. The Objective
/// is created through the Initiative aggregate root (<c>AddObjective</c>). Returns a
/// not-found error when the parent Initiative does not exist or belongs to another
/// tenant.
/// </summary>
public sealed class CreateObjectiveCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<ObjectiveResponse>> HandleAsync(
        CreateObjectiveCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<ObjectiveResponse>.Failure(tenant.Error);

        var initiative = await _dbContext.Initiatives
            .FirstOrDefaultAsync(
                i => i.Id == command.InitiativeId && i.TenantId == tenant.Value,
                cancellationToken);

        if (initiative is null)
            return Result<ObjectiveResponse>.Failure(
                InitiativeErrors.InitiativeNotFound(command.InitiativeId));

        var objective = initiative.AddObjective(command.Name, command.Description);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new ObjectiveResponse(
            objective.Id,
            objective.Name,
            objective.Description,
            objective.InitiativeId,
            objective.CreatedDate,
            objective.LastModifiedDate);

        return Result<ObjectiveResponse>.Created(response, $"/api/v1/objectives/{objective.Id}");
    }
}
