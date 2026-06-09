namespace ProgramPulse.Api.Infrastructure.SecurityHeaders;

public sealed class SecurityHeadersOption
{
    public const string SectionName = "SecurityHeaders";

    public bool Enabled { get; init; } = true;
    public string ContentTypeOptions { get; init; } = "nosniff";
    public string FrameOptions { get; init; } = "DENY";
    public string XssProtection { get; init; } = "0";
    public string StrictTransportSecurity { get; init; }
        = "max-age=63072000; includeSubDomains; preload";
    public string ReferrerPolicy { get; init; } = "strict-origin-when-cross-origin";
    public string PermissionsPolicy { get; init; }
        = "camera=(), microphone=(), geolocation=()";
}
