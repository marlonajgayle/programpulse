using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Measurements.Create;

public sealed record CreateMeasurementCommand(
    Guid KpiId,
    decimal Value,
    string? Notes);

/// <summary>
/// Records a new Measurement against a KPI the caller's tenant owns. The Measurement is
/// created through the KPI aggregate member (<c>AddMeasurement</c>), which also syncs
/// the KPI's current value to the new reading. Returns a not-found error when the
/// parent KPI does not exist or belongs to another tenant.
/// </summary>
public sealed class CreateMeasurementCommandHandler(
    ICurrentTenant currentTenant,
    IApplicationDbContext dbContext)
{
    private readonly ICurrentTenant _currentTenant = currentTenant;
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result<MeasurementResponse>> HandleAsync(
        CreateMeasurementCommand command,
        CancellationToken cancellationToken)
    {
        var tenant = await _currentTenant.GetTenantIdAsync(cancellationToken);
        if (tenant.IsFailure)
            return Result<MeasurementResponse>.Failure(tenant.Error);

        var kpi = await _dbContext.Kpis
            .FirstOrDefaultAsync(
                k => k.Id == command.KpiId && k.Objective.Programme.TenantId == tenant.Value,
                cancellationToken);

        if (kpi is null)
            return Result<MeasurementResponse>.Failure(KpiErrors.KpiNotFound(command.KpiId));

        var measurement = kpi.AddMeasurement(command.Value, command.Notes);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new MeasurementResponse(
            measurement.Id,
            measurement.Value,
            measurement.Notes,
            measurement.KpiId,
            measurement.CreatedDate,
            measurement.LastModifiedDate);

        return Result<MeasurementResponse>.Created(response, $"/api/v1/measurements/{measurement.Id}");
    }
}
