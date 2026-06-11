using System.Net;
using System.Net.Mail;
using FluentEmail.Core;

namespace ProgramPulse.Api.Infrastructure.Email;

/// <summary>
/// Wires up FluentEmail with the Razor renderer and an SMTP sender from the
/// bound <see cref="EmailOption"/>, and registers <see cref="IEmailService"/>.
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
            .AddRazorRenderer()
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

        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}
