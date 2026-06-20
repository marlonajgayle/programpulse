using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Measurements.Create;

/// <summary>
/// Records a new Measurement against the given KPI.
/// </summary>
public sealed class CreateMeasurementEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("kpis/{kpiId:guid}/measurements", async (
            Guid kpiId,
            CreateMeasurementCommand command,
            CreateMeasurementCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                command with { KpiId = kpiId }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<CreateMeasurementCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("CreateMeasurement")
        .WithTags("Measurements");
    }
}
