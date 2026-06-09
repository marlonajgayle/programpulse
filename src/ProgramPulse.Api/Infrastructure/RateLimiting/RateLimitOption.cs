namespace ProgramPulse.Api.Infrastructure.RateLimiting;

public sealed class RateLimitOption
{
    public const string SectionName = "IpRateLimiting";

    public bool Enabled { get; init; } = true;
    public int PermitLimit { get; init; } = 100;
    public int WindowSeconds { get; init; } = 60;
    public int QueueLimit { get; init; } = 0;
}
