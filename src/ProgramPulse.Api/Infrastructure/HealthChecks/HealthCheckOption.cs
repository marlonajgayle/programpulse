namespace ProgramPulse.Api.Infrastructure.HealthChecks;

public class HealthCheckOption
{
    public const string SectionName = "HealthChecks";

    public bool Enabled { get; set; } = true;
    public string Endpoint { get; set; } = "/health";
}
