namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// The access token issued to a client, returned in the response body. The token
/// itself is also written to an HttpOnly cookie by <see cref="IAuthCookieService"/>.
/// </summary>
public sealed class AccessTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public string Jti { get; set; } = string.Empty;
}
