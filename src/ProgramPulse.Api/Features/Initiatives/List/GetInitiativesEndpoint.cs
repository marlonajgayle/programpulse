using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Initiatives.List;

/// <summary>
/// Returns all Initiatives within the caller's tenant.
/// </summary>
public sealed class GetInitiativesEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("initiatives", async (
            GetInitiativesQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetInitiativesQuery(), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("GetInitiatives")
        .WithTags("Initiatives");
    }
}
