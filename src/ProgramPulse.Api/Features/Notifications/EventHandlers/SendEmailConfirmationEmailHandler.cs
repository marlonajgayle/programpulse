using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Email;
using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.EventHandlers;

/// <summary>
/// Sends the email-confirmation message when an <see cref="EmailConfirmationRequestedEvent"/> is
/// dispatched from the outbox. Throws on a failed send so the outbox records the error and
/// retries on the next poll.
/// </summary>
public sealed class SendEmailConfirmationEmailHandler(
    IEmailService emailService
) : IDomainEventHandler<EmailConfirmationRequestedEvent>
{
    public async Task HandleAsync(
        EmailConfirmationRequestedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var message = new EmailMessage
        {
            To = domainEvent.To,
            Subject = "Confirm your ProgramPulse email",
            IsHtml = false,
            Body =
                $"Dear {domainEvent.FirstName},\n\n" +
                "Please confirm your email address to activate your ProgramPulse account.\n\n" +
                $"Click the link below to confirm your email:\n{domainEvent.ConfirmLink}\n\n" +
                "If you did not create this account, please ignore this email.\n\n" +
                "Best regards,\n" +
                "The ProgramPulse Team"
        };

        var sent = await emailService.SendEmailAsync(message, cancellationToken);

        if (!sent)
        {
            throw new InvalidOperationException(
                $"Failed to send email confirmation email to '{domainEvent.To}'.");
        }
    }
}
