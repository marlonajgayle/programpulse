using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Measurements.List;

/// <summary>
/// Returns all Measurements for the given KPI.
/// </summary>
public sealed class GetMeasurementsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("kpis/{kpiId:guid}/measurements", async (
            Guid kpiId,
            GetMeasurementsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetMeasurementsQuery(kpiId), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("GetMeasurements")
        .WithTags("Measurements");
    }
}
