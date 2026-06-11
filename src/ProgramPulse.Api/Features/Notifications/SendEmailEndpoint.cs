using Asp.Versioning;
using ProgramPulse.Api.Infrastructure.Email;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Notifications;

/// <summary>
/// Direct-send test endpoint: sends an email synchronously via
/// <see cref="IEmailService"/>. Useful for smoke-testing SMTP configuration.
/// </summary>
public sealed class SendEmailEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("send-email", async (
            SendEmailRequest request,
            IEmailService emailService,
            CancellationToken cancellationToken) =>
        {
            var emailMessage = new EmailMessage
            {
                To = request.To,
                Subject = request.Subject,
                Body = request.Body,
                IsHtml = request.IsHtml
            };

            var result = await emailService.SendEmailAsync(emailMessage, cancellationToken);

            return result
                ? Results.Ok("Email sent successfully.")
                : Results.StatusCode(StatusCodes.Status500InternalServerError);
        })
        .HasApiVersion(ApiVersions.V1)
        .WithName("SendEmail")
        .WithTags("Notifications");
    }
}

public sealed record SendEmailRequest(
    string To,
    string Subject,
    string Body,
    bool IsHtml = true);
