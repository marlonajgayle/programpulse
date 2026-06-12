using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Email;
using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.EventHandlers;

/// <summary>
/// Sends the password-reset email when a <see cref="PasswordResetEmailRequestedEvent"/> is
/// dispatched from the outbox. Throws on a failed send so the outbox records the error and
/// retries on the next poll.
/// </summary>
public sealed class SendPasswordResetEmailHandler(
    IEmailService emailService
) : IDomainEventHandler<PasswordResetEmailRequestedEvent>
{
    public async Task HandleAsync(
        PasswordResetEmailRequestedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var message = new EmailMessage
        {
            To = domainEvent.To,
            Subject = "Reset Your ProgramPulse Password",
            IsHtml = false,
            Body =
                $"Dear {domainEvent.FirstName},\n\n" +
                "We received a request to reset your password for your ProgramPulse account.\n\n" +
                $"Click the link below to reset your password:\n{domainEvent.ResetLink}\n\n" +
                "If you did not request a password reset, please ignore this email.\n\n" +
                "Best regards,\n" +
                "The ProgramPulse Team"
        };

        var sent = await emailService.SendEmailAsync(message, cancellationToken);

        if (!sent)
        {
            throw new InvalidOperationException(
                $"Failed to send password reset email to '{domainEvent.To}'.");
        }
    }
}
