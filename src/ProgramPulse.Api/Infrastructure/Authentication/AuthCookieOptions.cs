namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Configures the HttpOnly cookies used to carry the access and refresh tokens.
/// Bound from the <c>AuthCookies</c> configuration section.
/// </summary>
public class AuthCookieOptions
{
    public const string SectionName = "AuthCookies";

    public string AccessTokenCookieName { get; set; } = "access_token";
    public string RefreshTokenCookieName { get; set; } = "refresh_token";
    public bool Secure { get; set; } = true;
    public string SameSite { get; set; } = "Strict";
    public string Path { get; set; } = "/";
}
