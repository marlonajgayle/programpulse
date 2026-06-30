using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Email;
using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.EventHandlers;

/// <summary>
/// Sends the welcome email when a <see cref="WelcomeEmailRequestedEvent"/> is
/// dispatched from the outbox. Throws on a failed send so the outbox records the
/// error and retries on the next poll.
/// </summary>
public sealed class SendWelcomeEmailHandler(
    IEmailService emailService
) : IDomainEventHandler<WelcomeEmailRequestedEvent>
{
    private const string TemplatePath =
        "~/Infrastructure/Email/Templates/WelcomeEmail.cshtml";

    public async Task HandleAsync(
        WelcomeEmailRequestedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var message = new TemplatedEmailMessage
        {
            To = domainEvent.To,
            Subject = "Welcome to ProgramPulse",
            TemplatePath = TemplatePath,
            Model = new WelcomeEmailModel { UserName = domainEvent.UserName }
        };

        var sent = await emailService.SendTemplatedEmailAsync(message, cancellationToken);

        if (!sent)
        {
            throw new InvalidOperationException(
                $"Failed to send welcome email to '{domainEvent.To}'.");
        }
    }
}
