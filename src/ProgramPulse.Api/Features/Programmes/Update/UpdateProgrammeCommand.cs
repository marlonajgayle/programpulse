using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Programmes.Update;

public sealed record UpdateProgrammeCommand(
    Guid Id,
    string Name,
    string Description,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? ParentProgrammeId = null);

/// <summary>
/// Updates an existing Programme within the caller's tenant. Returns a not-found
/// error when the Programme does not exist, belongs to another tenant, or has been
/// soft-deleted (excluded by the global query filter).
/// </summary>
public sealed class UpdateProgrammeCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        UpdateProgrammeCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result.Failure(tenant.Error);

        var programme = await _dbContext.Programmes
            .FirstOrDefaultAsync(
                i => i.Id == command.Id && i.TenantId == tenant.Value,
                cancellationToken);

        if (programme is null)
            return Result.Failure(ProgrammeErrors.ProgrammeNotFound(command.Id));

        var parentCheck = await ValidateParentAsync(command, tenant.Value, cancellationToken);
        if (parentCheck.IsFailure)
            return parentCheck;

        programme.Update(command.Name, command.Description, command.StartDate, command.EndDate);
        programme.SetParent(command.ParentProgrammeId);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    // A new parent must exist in-tenant, be top-level, and not be the programme itself.
    // The programme being edited must also have no sub-programmes of its own, otherwise
    // making it a child would push the hierarchy past two levels.
    private async Task<Result> ValidateParentAsync(
        UpdateProgrammeCommand command, Guid tenantId, CancellationToken cancellationToken)
    {
        if (command.ParentProgrammeId is not { } parentId)
            return Result.Success();

        if (parentId == command.Id)
            return Result.Failure(ProgrammeErrors.ParentIsSelf);

        var hasSubProgrammes = await _dbContext.Programmes
            .AsNoTracking()
            .AnyAsync(i => i.TenantId == tenantId && i.ParentProgrammeId == command.Id, cancellationToken);

        if (hasSubProgrammes)
            return Result.Failure(ProgrammeErrors.ProgrammeHasSubProgrammes);

        var parent = await _dbContext.Programmes
            .AsNoTracking()
            .Where(i => i.Id == parentId && i.TenantId == tenantId)
            .Select(i => new { i.ParentProgrammeId })
            .FirstOrDefaultAsync(cancellationToken);

        if (parent is null)
            return Result.Failure(ProgrammeErrors.ParentProgrammeNotFound(parentId));

        if (parent.ParentProgrammeId is not null)
            return Result.Failure(ProgrammeErrors.ParentNotTopLevel);

        return Result.Success();
    }
}
