using System.Diagnostics;
using Microsoft.Extensions.Primitives;
using ProgramPulse.Api.Infrastructure;
using Serilog;
using Serilog.Events;

namespace ProgramPulse.Api.Infrastructure.Logging;

public static class LoggingConfiguration
{
    public static void ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        hostBuilder.UseSerilog((ctx, loggerConf) =>
        {
            bool isDevelopment = ctx.HostingEnvironment.IsDevelopmentOrLocal();

            loggerConf
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)

                // ---- Enrichers ----
                .Enrich.FromLogContext()
                .Enrich.With(new SensitiveDataMaskingEnricher())
                .Enrich.WithEnvironmentName()
                .Enrich.WithProcessId()
                .Enrich.WithThreadId()

                // ---- API logs (exclude performance logs) ----
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.Properties.TryGetValue("LogSource", out var v) &&
                        v.ToString().Contains("API"))
                    .WriteTo.File(
                        "logs/programpulse-api-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        shared: true,
                        outputTemplate:
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} " +
                            "[{Level:u3}] " +
                            "{Message:lj} " +
                            "{Properties}{NewLine}{Exception}"))

                // ---- Background logs (exclude performance logs) ----
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.Properties.TryGetValue("LogSource", out var v) &&
                        v.ToString().Contains("Background"))
                    .WriteTo.File(
                        "logs/programpulse-background-.log",
                        rollingInterval: RollingInterval.Day,
                        retainedFileCountLimit: 30,
                        shared: true,
                        outputTemplate:
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} " +
                            "[{Level:u3}] " +
                            "{Message:lj} " +
                            "{Properties}{NewLine}{Exception}"))

                // ---- Performance logs ONLY ----
                .WriteTo.Logger(lc => lc
                    .Filter.ByIncludingOnly(e =>
                        e.MessageTemplate.Text.Contains("responded"))
                    .WriteTo.File(
                        "logs/programpulse-performance-.log",
                        rollingInterval: RollingInterval.Day,
                        outputTemplate:
                            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} " +
                            "{Level:u3} " +
                            "{HttpMethod} {RequestPath} " +
                            "{StatusCode} {DurationMs}ms " +
                            "{PerformanceLevel} " +
                            "{CorrelationId}{NewLine}"));

            // ---- Console (dev only) ----
            if (isDevelopment)
                loggerConf.WriteTo.Console(
                    outputTemplate:
                        "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} " +
                        "[{Level:u3}] {Message:lj} " +
                        "{Properties}{NewLine}{Exception}");
        });
    }

    public static WebApplication UseRequestPerformanceLogging(this WebApplication app)
    {
        // Stamp a start timestamp so the enrich callback can compute elapsed ms.
        app.Use(async (context, next) =>
        {
            context.Items["RequestStart"] = Stopwatch.GetTimestamp();
            await next();
        });

        app.UseSerilogRequestLogging(options =>
        {
            // Keep "responded" so the performance-sink filter matches; use the custom prop names.
            options.MessageTemplate =
                "HTTP {HttpMethod} {RequestPath} responded {StatusCode} in {DurationMs}ms {PerformanceLevel} {CorrelationId}";

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                double elapsedMs = httpContext.Items.TryGetValue("RequestStart", out var start) && start is long ticks
                    ? (Stopwatch.GetTimestamp() - ticks) * 1000.0 / Stopwatch.Frequency
                    : 0d;

                string correlationId =
                    httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out StringValues cid)
                    && !StringValues.IsNullOrEmpty(cid)
                        ? cid.ToString()
                        : httpContext.TraceIdentifier;

                diagnosticContext.Set("HttpMethod", httpContext.Request.Method);
                diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value);
                diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);
                diagnosticContext.Set("DurationMs", Math.Round(elapsedMs, 1));
                diagnosticContext.Set("PerformanceLevel", ClassifyPerformance(elapsedMs));
                diagnosticContext.Set("CorrelationId", correlationId);
            };
        });

        return app;
    }

    // Tune thresholds as desired.
    private static string ClassifyPerformance(double elapsedMs) => elapsedMs switch
    {
        < 300 => "Fast",
        < 1000 => "Normal",
        < 3000 => "Slow",
        _ => "Critical"
    };
}
