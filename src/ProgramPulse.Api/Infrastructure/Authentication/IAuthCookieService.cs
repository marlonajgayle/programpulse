namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Reads and writes the access/refresh token HttpOnly cookies on the current
/// request/response.
/// </summary>
public interface IAuthCookieService
{
    void SetAccessTokenCookie(HttpResponse response, string token, DateTime expiresAt);
    void SetRefreshTokenCookie(HttpResponse response, string token, DateTime expiresAt);
    void ClearAuthCookies(HttpResponse response);
    string? GetAccessTokenFromCookie(HttpRequest request);
    string? GetRefreshTokenFromCookie(HttpRequest request);
}
