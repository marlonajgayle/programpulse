using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Authentication.ChangePassword;

/// <summary>
/// Changes the authenticated caller's password. The new password and its confirmation are
/// validated up front by the request filter; the handler then verifies the current password
/// against ASP.NET Identity and applies the new one.
/// </summary>
public sealed class ChangePasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/change-password", async (
            ChangePasswordCommand command,
            ChangePasswordCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<ChangePasswordCommand>()
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("ChangePassword")
        .WithTags("Authentication");
    }
}
