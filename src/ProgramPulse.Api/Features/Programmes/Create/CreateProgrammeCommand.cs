using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Programmes.Create;

public sealed record CreateProgrammeCommand(
    string Name,
    string Description,
    DateTime? StartDate,
    DateTime? EndDate,
    Guid? ParentProgrammeId = null);

/// <summary>
/// Creates a new Programme within the caller's tenant. Audit fields are stamped
/// automatically on save by <c>ApplicationDbContext</c>.
/// </summary>
public sealed class CreateProgrammeCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<ProgrammeResponse>> HandleAsync(
        CreateProgrammeCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<ProgrammeResponse>.Failure(tenant.Error);

        if (command.ParentProgrammeId is { } parentId)
        {
            var parentCheck = await ValidateParentAsync(parentId, tenant.Value, cancellationToken);
            if (parentCheck.IsFailure)
                return Result<ProgrammeResponse>.Failure(parentCheck.Error);
        }

        var programme = Programme.Create(
            command.Name,
            command.Description,
            command.StartDate,
            command.EndDate,
            tenant.Value,
            command.ParentProgrammeId);

        _dbContext.Programmes.Add(programme);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new ProgrammeResponse(
            programme.Id,
            programme.Name,
            programme.Description,
            programme.StartDate,
            programme.EndDate,
            programme.Status,
            programme.CreatedDate,
            programme.LastModifiedDate,
            ObjectiveCount: 0,
            KpiCount: 0,
            ParentProgrammeId: programme.ParentProgrammeId);

        return Result<ProgrammeResponse>.Created(response, $"/api/v1/programmes/{programme.Id}");
    }

    // A parent must exist in the caller's tenant and itself be top-level, so the
    // hierarchy stays at most two levels deep (Programme → Sub-Programme).
    private async Task<Result> ValidateParentAsync(
        Guid parentId, Guid tenantId, CancellationToken cancellationToken)
    {
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
