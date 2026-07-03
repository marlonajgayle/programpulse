using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Initiatives.Create;

public sealed record CreateInitiativeCommand(
    string Name,
    string Description,
    DateTime StartDate,
    DateTime? EndDate);

/// <summary>
/// Creates a new Initiative within the caller's tenant. Audit fields are stamped
/// automatically on save by <c>ApplicationDbContext</c>.
/// </summary>
public sealed class CreateInitiativeCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<InitiativeResponse>> HandleAsync(
        CreateInitiativeCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<InitiativeResponse>.Failure(tenant.Error);

        var initiative = Initiative.Create(
            command.Name,
            command.Description,
            command.StartDate,
            command.EndDate,
            tenant.Value);

        _dbContext.Initiatives.Add(initiative);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new InitiativeResponse(
            initiative.Id,
            initiative.Name,
            initiative.Description,
            initiative.StartDate,
            initiative.EndDate,
            initiative.CreatedDate,
            initiative.LastModifiedDate,
            ObjectiveCount: 0,
            KpiCount: 0);

        return Result<InitiativeResponse>.Created(response, $"/api/v1/initiatives/{initiative.Id}");
    }
}
