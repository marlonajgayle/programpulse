using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ProgramPulse.Api.Infrastructure.RateLimiting;

public static class RateLimitingConfiguration
{
    public static IServiceCollection AddRateLimitingConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind rate limiting options
        services.Configure<RateLimitOption>(
            configuration.GetSection(RateLimitOption.SectionName));

        var options = configuration
            .GetSection(RateLimitOption.SectionName)
            .Get<RateLimitOption>() ?? new RateLimitOption();

        if (!options.Enabled)
            return services;

        services.AddRateLimiter(rateLimiter =>
        {
            rateLimiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            rateLimiter.OnRejected = async (context, cancellationToken) =>
            {
                var http = context.HttpContext;

                http.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                http.Response.ContentType = "application/problem+json";

                var windowSeconds = options.WindowSeconds.ToString(CultureInfo.InvariantCulture);

                http.Response.Headers.RetryAfter = windowSeconds;
                http.Response.Headers["RateLimit-Limit"] =
                    options.PermitLimit.ToString(CultureInfo.InvariantCulture);
                http.Response.Headers["RateLimit-Remaining"] = "0";
                http.Response.Headers["RateLimit-Reset"] = windowSeconds;

                var problem = new ProblemDetails
                {
                    Title = "rate_limit_exceeded",
                    Detail = $"Too many requests. Please retry after {windowSeconds} seconds.",
                    Status = StatusCodes.Status429TooManyRequests,
                    Instance = http.Request.Path
                };

                await http.Response.WriteAsJsonAsync(problem, cancellationToken);
            };

            rateLimiter.AddPolicy(RateLimitPolicies.IpFixedWindow, context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString()
                         ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = options.PermitLimit,
                        Window = TimeSpan.FromSeconds(options.WindowSeconds),
                        QueueLimit = options.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });
        });

        return services;
    }

    public static IApplicationBuilder UseRateLimitingConfiguration(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var options = configuration
            .GetSection(RateLimitOption.SectionName)
            .Get<RateLimitOption>() ?? new RateLimitOption();

        if (!options.Enabled)
            return app;

        app.UseRateLimiter();

        return app;
    }
}
