namespace ProgramPulse.Api.Infrastructure.Email;

/// <summary>
/// SMTP/sender settings for the FluentEmail integration. Bound from the
/// <c>EmailOptions</c> configuration section. Real credentials should come from
/// environment variables / user-secrets per environment, never source control.
/// </summary>
public sealed class EmailOption
{
    public const string SectionName = "EmailOptions";

    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string DefaultFromEmail { get; set; } = string.Empty;
    public string DefaultFromName { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
}
