using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public static class MeasurementErrors
{
    public static Error MeasurementNotFound(Guid measurementId) => Error.NotFound(
        code: "Measurement.NotFound",
        message: $"Measurement with ID '{measurementId}' was not found.");

    public static Error MeasurementTooSoon(MeasurementFrequency frequency) => Error.Conflict(
        code: "Measurement.TooSoon",
        message: $"A measurement already exists within the expected {frequency} interval for this KPI.");
}
