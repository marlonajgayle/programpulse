using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Tenants.Programmes;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Objectives.Create;

public sealed record CreateObjectiveCommand(
    Guid ProgrammeId,
    string Name,
    string Description,
    CreateObjectiveKpi Kpi);

/// <summary>
/// The single KPI an objective is created with. An objective always has exactly one KPI.
/// </summary>
public sealed record CreateObjectiveKpi(
    string Name,
    string Unit,
    KpiDirection Direction,
    decimal BaselineValue,
    decimal TargetValue,
    decimal CurrentValue,
    DateTime DueDate,
    MeasurementFrequency? Frequency);

/// <summary>
/// Creates a new Objective (with its single KPI) under a Programme the caller's tenant
/// owns. The Objective is created through the Programme aggregate root (<c>AddObjective</c>),
/// which builds the KPI inline. Returns a not-found error when the parent Programme does
/// not exist or belongs to another tenant.
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

        var programme = await _dbContext.Programmes
            .FirstOrDefaultAsync(
                i => i.Id == command.ProgrammeId && i.TenantId == tenant.Value,
                cancellationToken);

        if (programme is null)
            return Result<ObjectiveResponse>.Failure(
                ProgrammeErrors.ProgrammeNotFound(command.ProgrammeId));

        var objective = programme.AddObjective(
            command.Name,
            command.Description,
            command.Kpi.Name,
            command.Kpi.Unit,
            command.Kpi.Direction,
            command.Kpi.BaselineValue,
            command.Kpi.TargetValue,
            command.Kpi.CurrentValue,
            command.Kpi.DueDate,
            command.Kpi.Frequency);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var response = new ObjectiveResponse(
            objective.Id,
            objective.Name,
            objective.Description,
            objective.ProgrammeId,
            objective.CreatedDate,
            objective.LastModifiedDate);

        return Result<ObjectiveResponse>.Created(response, $"/api/v1/objectives/{objective.Id}");
    }
}
