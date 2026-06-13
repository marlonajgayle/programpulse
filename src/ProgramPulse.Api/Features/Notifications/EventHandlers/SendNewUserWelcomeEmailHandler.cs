using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Email;
using ProgramPulse.Api.SharedKernel;

namespace ProgramPulse.Api.Features.Notifications.EventHandlers;

/// <summary>
/// Sends the welcome email (including the temporary password) when a
/// <see cref="NewUserWelcomeEmailRequestedEvent"/> is dispatched from the outbox.
/// Throws on a failed send so the outbox records the error and retries on the
/// next poll.
/// </summary>
public sealed class SendNewUserWelcomeEmailHandler(
    IEmailService emailService
) : IDomainEventHandler<NewUserWelcomeEmailRequestedEvent>
{
    private static readonly string TemplatePath = Path.Combine(
        AppContext.BaseDirectory,
        "Infrastructure", "Email", "Templates", "NewUserWelcomeEmail.cshtml");

    public async Task HandleAsync(
        NewUserWelcomeEmailRequestedEvent domainEvent,
        CancellationToken cancellationToken)
    {
        var message = new TemplatedEmailMessage
        {
            To = domainEvent.To,
            Subject = "Your ProgramPulse account is ready",
            TemplatePath = TemplatePath,
            Model = new NewUserWelcomeEmailModel
            {
                FirstName = domainEvent.FirstName,
                TemporaryPassword = domainEvent.TemporaryPassword
            }
        };

        var sent = await emailService.SendTemplatedEmailAsync(message, cancellationToken);

        if (!sent)
        {
            throw new InvalidOperationException(
                $"Failed to send new-user welcome email to '{domainEvent.To}'.");
        }
    }
}
