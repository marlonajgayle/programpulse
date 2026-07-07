namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public static class MeasurementFrequencyExtensions
{
    /// <summary>
    /// Shifts <paramref name="date"/> by one interval of the given frequency.
    /// <paramref name="sign"/> is +1 (forward) or -1 (backward), used to build the
    /// rolling window that limits how close together measurement entries may fall.
    /// </summary>
    public static DateTime AddInterval(this MeasurementFrequency frequency, DateTime date, int sign) => frequency switch
    {
        MeasurementFrequency.Weekly => date.AddDays(7 * sign),
        MeasurementFrequency.Monthly => date.AddMonths(1 * sign),
        MeasurementFrequency.Quarterly => date.AddMonths(3 * sign),
        MeasurementFrequency.BiAnnually => date.AddMonths(6 * sign),
        MeasurementFrequency.Annually => date.AddYears(1 * sign),
        _ => throw new ArgumentOutOfRangeException(nameof(frequency), frequency, "Unknown measurement frequency.")
    };
}
