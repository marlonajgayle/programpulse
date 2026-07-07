using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Programmes.Create;

/// <summary>
/// Creates a new Programme within the caller's tenant.
/// </summary>
public sealed class CreateProgrammeEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("programmes", async (
            CreateProgrammeCommand command,
            CreateProgrammeCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<CreateProgrammeCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("CreateProgramme")
        .WithTags("Programmes");
    }
}
