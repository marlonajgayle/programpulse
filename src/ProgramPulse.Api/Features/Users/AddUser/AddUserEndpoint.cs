using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Users.AddUser;

/// <summary>
/// Admin-only endpoint that lets a tenant administrator add a new user to their
/// own tenant. Returns 201 Created with the new user identifier.
/// </summary>
public sealed class AddUserEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users", async (
            AddUserCommand command,
            AddUserCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<AddUserCommand>()
        .RequireAuthorization(AuthorizationPolicies.AdminOnly)
        .WithName("AddUser")
        .WithTags("Users");
    }
}
