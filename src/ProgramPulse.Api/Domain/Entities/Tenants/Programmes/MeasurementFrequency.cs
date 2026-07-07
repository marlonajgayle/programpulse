namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

/// <summary>
/// The expected interval at which a KPI is measured. Used to limit how frequently
/// measurement entries can be recorded (see <see cref="MeasurementFrequencyExtensions"/>).
/// </summary>
public enum MeasurementFrequency
{
    Weekly,
    Monthly,
    Quarterly,
    BiAnnually,
    Annually
}
