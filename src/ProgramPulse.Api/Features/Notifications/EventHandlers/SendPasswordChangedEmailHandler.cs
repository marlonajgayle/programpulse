using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Email;
using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.EventHandlers;

/// <summary>
/// Sends the password-change confirmation email when a <see cref="PasswordChangedEmailRequestedEvent"/>
/// is dispatched from the outbox. Throws on a failed send so the outbox records the error and
/// retries on the next poll.
/// </summary>
public sealed class SendPasswordChangedEmailHandler(
    IEmailService emailService
) : IDomainEventHandler<PasswordChangedEmailRequestedEvent>
{
    public async Task HandleAsync(
        PasswordChangedEmailRequestedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var message = new EmailMessage
        {
            To = domainEvent.To,
            Subject = "Your ProgramPulse Password Was Changed",
            IsHtml = false,
            Body =
                $"Dear {domainEvent.FirstName},\n\n" +
                "This is a confirmation that the password for your ProgramPulse account was " +
                "just changed.\n\n" +
                "If you made this change, no further action is needed.\n\n" +
                "If you did NOT change your password, please reset it immediately and contact " +
                "support, as your account may be compromised.\n\n" +
                "Best regards,\n" +
                "The ProgramPulse Team"
        };

        var sent = await emailService.SendEmailAsync(message, cancellationToken);

        if (!sent)
        {
            throw new InvalidOperationException(
                $"Failed to send password change confirmation email to '{domainEvent.To}'.");
        }
    }
}
