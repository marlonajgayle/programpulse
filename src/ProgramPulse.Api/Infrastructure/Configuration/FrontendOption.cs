namespace ProgramPulse.Api.Infrastructure.Configuration;

/// <summary>
/// Settings for the frontend SPA. Bound from the <c>Frontend</c> configuration section.
/// <see cref="BaseUrl"/> is the public origin of the Blazor app (e.g. <c>https://localhost:7208</c>)
/// and is used to build user-facing links — such as the password-reset link emailed to users —
/// so they point at the SPA rather than the API host. Set per environment in appsettings.&lt;env&gt;.json.
/// </summary>
public sealed class FrontendOption
{
    public const string SectionName = "Frontend";

    public string BaseUrl { get; set; } = string.Empty;
}
