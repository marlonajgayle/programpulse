namespace ProgramPulse.Api.Infrastructure.Email;

/// <summary>
/// Sends emails via FluentEmail. Returns <c>true</c> on a successful send,
/// <c>false</c> when the provider reports a failure or an exception is caught.
/// </summary>
public interface IEmailService
{
    Task<bool> SendEmailAsync(
        EmailMessage emailMessage,
        CancellationToken cancellationToken = default);

    Task<bool> SendEmailWithAttachmentsAsync(
        EmailMessageWithAttachment emailMessageWithAttachment,
        CancellationToken cancellationToken = default);

    Task<bool> SendTemplatedEmailAsync(
        TemplatedEmailMessage templatedEmailMessage,
        CancellationToken cancellationToken = default);
}
