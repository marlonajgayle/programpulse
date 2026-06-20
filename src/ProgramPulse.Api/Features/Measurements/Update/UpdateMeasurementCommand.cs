using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Measurements.Update;

public sealed record UpdateMeasurementCommand(Guid Id, decimal Value, string? Notes);

/// <summary>
/// Updates a Measurement the caller's tenant owns (verified via the KPI → Objective →
/// Initiative tenant chain). Returns a not-found error when the Measurement does not
/// exist, belongs to another tenant, or has been soft-deleted.
/// </summary>
public sealed class UpdateMeasurementCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        UpdateMeasurementCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result.Failure(tenant.Error);

        var measurement = await _dbContext.Measurements
            .FirstOrDefaultAsync(
                m => m.Id == command.Id
                    && m.Kpi.Objective.Initiative.TenantId == tenant.Value,
                cancellationToken);

        if (measurement is null)
            return Result.Failure(MeasurementErrors.MeasurementNotFound(command.Id));

        measurement.Update(command.Value, command.Notes);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
