using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Initiatives.Update;

public sealed record UpdateInitiativeCommand(
    Guid Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime? EndDate);

/// <summary>
/// Updates an existing Initiative within the caller's tenant. Returns a not-found
/// error when the Initiative does not exist, belongs to another tenant, or has been
/// soft-deleted (excluded by the global query filter).
/// </summary>
public sealed class UpdateInitiativeCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        UpdateInitiativeCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result.Failure(tenant.Error);

        var initiative = await _dbContext.Initiatives
            .FirstOrDefaultAsync(
                i => i.Id == command.Id && i.TenantId == tenant.Value,
                cancellationToken);

        if (initiative is null)
            return Result.Failure(InitiativeErrors.InitiativeNotFound(command.Id));

        initiative.Update(command.Name, command.Description, command.StartDate, command.EndDate);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
