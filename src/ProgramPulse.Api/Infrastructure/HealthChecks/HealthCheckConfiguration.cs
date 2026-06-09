using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace ProgramPulse.Api.Infrastructure.HealthChecks;

public static class HealthCheckConfiguration
{
    public static IServiceCollection AddHealthCheckConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind health check options
        services.Configure<HealthCheckOption>(
            configuration.GetSection(HealthCheckOption.SectionName));

        var healthCheckOption = configuration
            .GetSection(HealthCheckOption.SectionName)
            .Get<HealthCheckOption>() ?? new HealthCheckOption();

        if (!healthCheckOption.Enabled)
            return services;

        // Register health checks
        services.AddHealthChecks()
            .AddCheck<ApplicationHealthCheck>("application_health_check");

        return services;
    }

    public static IApplicationBuilder UseHealthCheckConfiguration(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var healthCheckOption = configuration
            .GetSection(HealthCheckOption.SectionName)
            .Get<HealthCheckOption>() ?? new HealthCheckOption();

        if (!healthCheckOption.Enabled)
            return app;

        // Map health check endpoint with custom response writer
        app.UseHealthChecks(healthCheckOption.Endpoint, new HealthCheckOptions
        {
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var response = new HealthCheckResponse
                {
                    OverallStatus = report.Status.ToString(),
                    TotalDuration = report.TotalDuration.TotalSeconds.ToString("0.00"),
                    HealthChecks = report.Entries.Select(entry => new HealthCheckItem(
                        entry.Value.Status.ToString(),
                        entry.Key,
                        entry.Value.Description ?? string.Empty,
                        entry.Value.Duration.TotalSeconds.ToString("0.00")))
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
            }
        });

        return app;
    }
}
