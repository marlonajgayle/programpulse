using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Initiatives.Create;

/// <summary>
/// Creates a new Initiative within the caller's tenant.
/// </summary>
public sealed class CreateInitiativeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("initiatives", async (
            CreateInitiativeCommand command,
            CreateInitiativeCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<CreateInitiativeCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("CreateInitiative")
        .WithTags("Initiatives");
    }
}
