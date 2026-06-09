using Serilog.Core;
using Serilog.Events;

namespace ProgramPulse.Api.Infrastructure.Logging;

/// <summary>
/// Redacts log event property values whose key looks sensitive (e.g. contains
/// "password" or "token"). The event itself is preserved so surrounding diagnostic
/// context is kept; only the offending value is masked.
/// </summary>
public sealed class SensitiveDataMaskingEnricher : ILogEventEnricher
{
    private const string RedactedValue = "***REDACTED***";

    private static readonly string[] SensitiveKeyFragments =
    {
        "password",
        "token",
        "secret",
        "apikey",
        "api-key",
        "authorization",
        "credential",
        "connectionstring",
        "pwd",
    };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // Snapshot the keys first — AddOrUpdateProperty mutates the underlying
        // dictionary, so we must not enumerate it while updating.
        foreach (string key in logEvent.Properties.Keys.ToList())
        {
            if (IsSensitive(key))
            {
                logEvent.AddOrUpdateProperty(
                    propertyFactory.CreateProperty(key, RedactedValue));
            }
        }
    }

    private static bool IsSensitive(string key) =>
        SensitiveKeyFragments.Any(fragment =>
            key.Contains(fragment, StringComparison.OrdinalIgnoreCase));
}
