using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Authentication.Register;

/// <summary>
/// Public registration endpoint. Creates a new tenant together with its first administrator
/// in a single transaction and enqueues a welcome email. Returns 201 Created with the new
/// tenant and user identifiers.
/// </summary>
public sealed class RegisterEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async (
            RegisterCommand command,
            RegisterCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<RegisterCommand>()
        .WithName("Register")
        .WithTags("Authentication");
    }
}
