using FluentValidation;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Authentication.ConfirmEmail;

/// <summary>
/// Confirms a user's email from the link sent to their inbox. Public (anonymous) because the
/// recipient is not yet signed in. The email and token are carried as query-string parameters,
/// so the command is assembled and validated inline rather than via the body-bound
/// <c>WithValidation</c> filter.
/// </summary>
public sealed class ConfirmEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("auth/confirm-email", async (
            string email,
            string token,
            ConfirmEmailCommandHandler handler,
            IValidator<ConfirmEmailCommand> validator,
            CancellationToken cancellationToken) =>
        {
            var command = new ConfirmEmailCommand(email, token);

            var validation = await validator.ValidateAsync(command, cancellationToken);
            if (!validation.IsValid)
            {
                return Result<string>
                    .Failure(AuthenticationErrors.Validation(validation.ToErrors()))
                    .ToHttpResult();
            }

            var result = await handler.HandleAsync(command, cancellationToken);
            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .AllowAnonymous()
        .WithName("ConfirmEmail")
        .WithTags("Authentication");
    }
}
