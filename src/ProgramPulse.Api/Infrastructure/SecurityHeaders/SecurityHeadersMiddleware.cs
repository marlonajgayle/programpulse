using Microsoft.Extensions.Options;

namespace ProgramPulse.Api.Infrastructure.SecurityHeaders;

public sealed class SecurityHeadersMiddleware(
    RequestDelegate next,
    IOptions<SecurityHeadersOption> options)
{
    private readonly RequestDelegate _next = next;
    private readonly SecurityHeadersOption _options = options.Value;

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME sniffing
        headers.TryAdd("X-Content-Type-Options", _options.ContentTypeOptions);

        // Clickjacking protection
        headers.TryAdd("X-Frame-Options", _options.FrameOptions);

        // XSS protection (legacy but still useful)
        headers.TryAdd("X-XSS-Protection", _options.XssProtection);

        // Enforce HTTPS
        headers.TryAdd("Strict-Transport-Security", _options.StrictTransportSecurity);

        // Referrer policy
        headers.TryAdd("Referrer-Policy", _options.ReferrerPolicy);

        // Permissions policy (tighten as needed)
        headers.TryAdd("Permissions-Policy", _options.PermissionsPolicy);

        // Content Security Policy (adjust per app, enable when ready)
        /*headers.TryAdd(
            "Content-Security-Policy",
            "default-src 'self'; " +
            "object-src 'none'; " +
            "frame-ancestors 'none'; " +
            "base-uri 'self'; " +
            "img-src 'self' data:; " +
            "script-src 'self'; " +
            "style-src 'self' 'unsafe-inline'");*/

        await _next(context);
    }
}
