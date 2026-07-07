using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Initiatives.GetById;

/// <summary>
/// Returns a single Initiative within the caller's tenant, with its objectives and KPIs.
/// </summary>
public sealed class GetInitiativeByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("initiatives/{id:guid}", async (
            Guid id,
            GetInitiativeByIdQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetInitiativeByIdQuery(id), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("GetInitiativeById")
        .WithTags("Initiatives");
    }
}
