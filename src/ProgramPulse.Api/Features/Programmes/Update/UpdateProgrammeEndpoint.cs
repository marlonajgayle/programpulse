using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Initiatives.Update;

/// <summary>
/// Updates an existing Initiative. The id comes from the route; the remaining fields
/// come from the request body.
/// </summary>
public sealed class UpdateInitiativeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("initiatives/{id:guid}", async (
            Guid id,
            UpdateInitiativeCommand command,
            UpdateInitiativeCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command with { Id = id }, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<UpdateInitiativeCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("UpdateInitiative")
        .WithTags("Initiatives");
    }
}
