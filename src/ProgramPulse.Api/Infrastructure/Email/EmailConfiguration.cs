using System.Net;
using System.Net.Mail;
using FluentEmail.Core;
using Razor.Templating.Core;

namespace ProgramPulse.Api.Infrastructure.Email;

/// <summary>
/// Wires up FluentEmail with an SMTP sender from the bound <see cref="EmailOption"/>,
/// the Razor.Templating.Core view engine (for rendering email templates to HTML),
/// and registers <see cref="IEmailService"/>.
/// </summary>
public static class EmailConfiguration
{
    public static IServiceCollection AddEmailConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var emailSettings = configuration
            .GetSection(EmailOption.SectionName)
            .Get<EmailOption>() ?? new EmailOption();

        services.Configure<EmailOption>(
            configuration.GetSection(EmailOption.SectionName));

        services
            .AddFluentEmail(emailSettings.DefaultFromEmail, emailSettings.DefaultFromName)
            .AddSmtpSender(() => new SmtpClient(emailSettings.SmtpHost)
            {
                Port = emailSettings.SmtpPort,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    emailSettings.SmtpUsername,
                    emailSettings.SmtpPassword),
                EnableSsl = emailSettings.EnableSsl
            });

        // Razor view engine used to render the .cshtml email templates to HTML.
        services.AddRazorTemplating();

        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
