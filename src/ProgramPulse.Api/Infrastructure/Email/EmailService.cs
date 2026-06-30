using FluentEmail.Core;
using FluentEmail.Core.Models;
using Razor.Templating.Core;

namespace ProgramPulse.Api.Infrastructure.Email;

/// <summary>
/// FluentEmail-backed <see cref="IEmailService"/>. Wraps each send in
/// structured logging and never throws — callers branch on the boolean result.
/// Templated emails are rendered to HTML with <see cref="IRazorTemplateEngine"/>.
/// </summary>
public sealed class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly IRazorTemplateEngine _razorEngine;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IFluentEmail fluentEmail,
        IRazorTemplateEngine razorEngine,
        ILogger<EmailService> logger)
    {
        _fluentEmail = fluentEmail;
        _razorEngine = razorEngine;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(
        EmailMessage emailMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email to {To} with subject: {Subject}",
                emailMessage.To, emailMessage.Subject);

            var email = _fluentEmail
                .To(emailMessage.To)
                .Subject(emailMessage.Subject)
                .Body(emailMessage.Body, emailMessage.IsHtml);

            if (!string.IsNullOrEmpty(emailMessage.From))
            {
                email.SetFrom(emailMessage.From);
            }

            if (emailMessage.Cc?.Count > 0)
            {
                foreach (var cc in emailMessage.Cc)
                {
                    email.CC(cc);
                }
            }

            if (emailMessage.Bcc?.Count > 0)
            {
                foreach (var bcc in emailMessage.Bcc)
                {
                    email.BCC(bcc);
                }
            }

            var response = await email.SendAsync(cancellationToken);

            if (response.Successful)
            {
                _logger.LogInformation("Email sent successfully to {To}", emailMessage.To);
                return true;
            }

            _logger.LogError("Failed to send email to {To}. Errors: {Errors}",
                emailMessage.To, string.Join(", ", response.ErrorMessages));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email to {To}", emailMessage.To);
            return false;
        }
    }

    public async Task<bool> SendEmailWithAttachmentsAsync(
        EmailMessageWithAttachment emailMessageWithAttachment,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email with {AttachmentCount} attachments to {To}",
                emailMessageWithAttachment.Attachments.Count, emailMessageWithAttachment.To);

            var email = _fluentEmail
                .To(emailMessageWithAttachment.To)
                .Subject(emailMessageWithAttachment.Subject)
                .Body(emailMessageWithAttachment.Body, emailMessageWithAttachment.IsHtml);

            if (!string.IsNullOrEmpty(emailMessageWithAttachment.From))
            {
                email.SetFrom(emailMessageWithAttachment.From);
            }

            foreach (var attachment in emailMessageWithAttachment.Attachments)
            {
                if (attachment.Data != null)
                {
                    email.Attach(new Attachment
                    {
                        Data = attachment.Data,
                        Filename = attachment.FileName,
                        ContentType = attachment.ContentType
                    });
                }
                else if (!string.IsNullOrEmpty(attachment.FilePath))
                {
                    email.AttachFromFilename(
                        attachment.FilePath,
                        attachment.ContentType,
                        attachment.FileName);
                }
            }

            var response = await email.SendAsync(cancellationToken);

            if (response.Successful)
            {
                _logger.LogInformation("Email with attachments sent successfully to {To}",
                    emailMessageWithAttachment.To);
                return true;
            }

            _logger.LogError("Failed to send email with attachments to {To}. Errors: {Errors}",
                emailMessageWithAttachment.To, string.Join(", ", response.ErrorMessages));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending email with attachments to {To}",
                emailMessageWithAttachment.To);
            return false;
        }
    }

    public async Task<bool> SendTemplatedEmailAsync(
        TemplatedEmailMessage templatedEmailMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending templated email to {To} using template: {Template}",
                templatedEmailMessage.To, templatedEmailMessage.TemplatePath);

            var body = await _razorEngine.RenderAsync(
                templatedEmailMessage.TemplatePath, templatedEmailMessage.Model);

            var email = _fluentEmail
                .To(templatedEmailMessage.To)
                .Subject(templatedEmailMessage.Subject)
                .Body(body, isHtml: true);

            if (!string.IsNullOrEmpty(templatedEmailMessage.From))
            {
                email.SetFrom(templatedEmailMessage.From);
            }

            var response = await email.SendAsync(cancellationToken);

            if (response.Successful)
            {
                _logger.LogInformation("Templated email sent successfully to {To}",
                    templatedEmailMessage.To);
                return true;
            }

            _logger.LogError("Failed to send templated email to {To}. Errors: {Errors}",
                templatedEmailMessage.To, string.Join(", ", response.ErrorMessages));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while sending templated email to {To}",
                templatedEmailMessage.To);
            return false;
        }
    }
}
