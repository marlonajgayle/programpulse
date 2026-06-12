using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Authentication.ForgotPassword;

/// <summary>
/// Initiates the password-reset process. Always responds with the same generic message,
/// regardless of whether the email exists or is confirmed, to avoid account enumeration.
/// The reset link is delivered out-of-band by email via the outbox.
/// </summary>
public sealed class ForgotPasswordEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/forgot-password", async (
            ForgotPasswordCommand command,
            ForgotPasswordCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<ForgotPasswordCommand>()
        .WithName("ForgotPassword")
        .WithTags("Authentication");
    }
}
