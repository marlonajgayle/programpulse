using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.MeasurementComments.List;

/// <summary>
/// Returns all comments for the given Measurement.
/// </summary>
public sealed class GetMeasurementCommentsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("measurements/{measurementId:guid}/comments", async (
            Guid measurementId,
            GetMeasurementCommentsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new GetMeasurementCommentsQuery(measurementId), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("GetMeasurementComments")
        .WithTags("MeasurementComments");
    }
}
