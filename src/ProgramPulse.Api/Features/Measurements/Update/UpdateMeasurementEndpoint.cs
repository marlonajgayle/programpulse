using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Measurements.Update;

/// <summary>
/// Updates an existing Measurement. The id comes from the route.
/// </summary>
public sealed class UpdateMeasurementEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("measurements/{id:guid}", async (
            Guid id,
            UpdateMeasurementCommand command,
            UpdateMeasurementCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command with { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<UpdateMeasurementCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("UpdateMeasurement")
        .WithTags("Measurements");
    }
}
