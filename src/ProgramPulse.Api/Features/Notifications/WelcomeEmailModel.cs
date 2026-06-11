namespace ProgramPulse.Api.Features.Notifications;

/// <summary>
/// View model bound to <c>Infrastructure/Email/Templates/WelcomeEmail.cshtml</c>.
/// </summary>
public sealed class WelcomeEmailModel
{
    public string UserName { get; set; } = string.Empty;
}
