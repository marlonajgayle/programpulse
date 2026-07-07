using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Programmes.GetById;

/// <summary>
/// Returns a single Programme within the caller's tenant, with its objectives and KPIs.
/// </summary>
public sealed class GetProgrammeByIdEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("programmes/{id:guid}", async (
            Guid id,
            GetProgrammeByIdQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetProgrammeByIdQuery(id), cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("GetProgrammeById")
        .WithTags("Programmes");
    }
}
