using System.Reflection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProgramPulse.Api.Infrastructure.HealthChecks;

public sealed class ApplicationHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        return Task.FromResult(HealthCheckResult.Healthy($"Build {version}"));
    }
}
