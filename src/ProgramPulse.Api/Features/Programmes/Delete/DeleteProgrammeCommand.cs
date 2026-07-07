using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Programmes.Delete;

public sealed record DeleteProgrammeCommand(Guid Id);

/// <summary>
/// Soft-deletes an Programme within the caller's tenant. Returns a not-found error
/// when the Programme does not exist, belongs to another tenant, or has already been
/// deleted (excluded by the global query filter).
/// </summary>
public sealed class DeleteProgrammeCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        DeleteProgrammeCommand command,
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

        programme.MarkAsDeleted();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
