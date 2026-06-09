namespace ProgramPulse.Api.Infrastructure.HealthChecks;

public class HealthCheckResponse
{
    public string OverallStatus { get; set; } = string.Empty;
    public IEnumerable<HealthCheckItem> HealthChecks { get; set; } = [];
    public string TotalDuration { get; set; } = string.Empty;
}

public record HealthCheckItem(
    string Status,
    string Component,
    string Description,
    string Duration);
