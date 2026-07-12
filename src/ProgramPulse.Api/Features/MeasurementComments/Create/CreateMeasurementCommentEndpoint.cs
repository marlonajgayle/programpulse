using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.MeasurementComments.Create;

/// <summary>
/// Adds a comment to a Measurement. The measurement id comes from the route.
/// </summary>
public sealed class CreateMeasurementCommentEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("measurements/{measurementId:guid}/comments", async (
            Guid measurementId,
            CreateMeasurementCommentCommand command,
            CreateMeasurementCommentCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                command with { MeasurementId = measurementId }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<CreateMeasurementCommentCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("CreateMeasurementComment")
        .WithTags("MeasurementComments");
    }
}
