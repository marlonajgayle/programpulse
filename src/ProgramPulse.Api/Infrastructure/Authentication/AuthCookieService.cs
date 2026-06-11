using Microsoft.Extensions.Options;

namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <inheritdoc />
public class AuthCookieService : IAuthCookieService
{
    private readonly AuthCookieOptions _options;

    public AuthCookieService(IOptions<AuthCookieOptions> options)
    {
        _options = options.Value;
    }

    public void SetAccessTokenCookie(HttpResponse response, string token, DateTime expiresAt)
    {
        var cookieOptions = CreateCookieOptions(expiresAt);
        response.Cookies.Append(_options.AccessTokenCookieName, token, cookieOptions);
    }

    public void SetRefreshTokenCookie(HttpResponse response, string token, DateTime expiresAt)
    {
        var cookieOptions = CreateCookieOptions(expiresAt);
        response.Cookies.Append(_options.RefreshTokenCookieName, token, cookieOptions);
    }

    public void ClearAuthCookies(HttpResponse response)
    {
        var expiredCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _options.Secure,
            SameSite = ParseSameSiteMode(_options.SameSite),
            Path = _options.Path,
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        response.Cookies.Delete(_options.AccessTokenCookieName, expiredCookieOptions);
        response.Cookies.Delete(_options.RefreshTokenCookieName, expiredCookieOptions);
    }

    public string? GetAccessTokenFromCookie(HttpRequest request)
    {
        return request.Cookies[_options.AccessTokenCookieName];
    }

    public string? GetRefreshTokenFromCookie(HttpRequest request)
    {
        return request.Cookies[_options.RefreshTokenCookieName];
    }

    private CookieOptions CreateCookieOptions(DateTime expiresAt)
    {
        return new CookieOptions
        {
            HttpOnly = true,
            Secure = _options.Secure,
            SameSite = ParseSameSiteMode(_options.SameSite),
            Path = _options.Path,
            Expires = expiresAt
        };
    }

    private static SameSiteMode ParseSameSiteMode(string sameSite)
    {
        return sameSite.ToLowerInvariant() switch
        {
            "strict" => SameSiteMode.Strict,
            "lax" => SameSiteMode.Lax,
            "none" => SameSiteMode.None,
            _ => SameSiteMode.Strict
        };
    }
}
