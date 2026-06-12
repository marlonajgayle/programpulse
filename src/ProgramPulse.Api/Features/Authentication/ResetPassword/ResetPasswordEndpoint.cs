using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Authentication.ResetPassword;

/// <summary>
/// Resets a user's password using the emailed reset token. The token and matching password
/// confirmation are validated up front by the request filter; the handler then verifies the
/// token against ASP.NET Identity and applies the new password.
/// </summary>
public sealed class ResetPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/reset-password", async (
            ResetPasswordCommand command,
            ResetPasswordCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<ResetPasswordCommand>()
        .WithName("ResetPassword")
        .WithTags("Authentication");
    }
}
