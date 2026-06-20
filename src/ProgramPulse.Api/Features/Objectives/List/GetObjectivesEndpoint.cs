using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Objectives.List;

/// <summary>
/// Returns all Objectives under the given Initiative.
/// </summary>
public sealed class GetObjectivesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("initiatives/{initiativeId:guid}/objectives", async (
            Guid initiativeId,
            GetObjectivesQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(
                new GetObjectivesQuery(initiativeId), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("GetObjectives")
        .WithTags("Objectives");
    }
}
