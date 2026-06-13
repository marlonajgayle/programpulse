namespace ProgramPulse.Api.Features.Notifications;

/// <summary>
/// View model bound to <c>Infrastructure/Email/Templates/NewUserWelcomeEmail.cshtml</c>.
/// </summary>
public sealed class NewUserWelcomeEmailModel
{
    public string FirstName { get; set; } = string.Empty;

    public string TemporaryPassword { get; set; } = string.Empty;
}
