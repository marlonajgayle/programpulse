using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

public static class MeasurementErrors
{
    public static Error MeasurementNotFound(Guid measurementId) => Error.NotFound(
        code: "Measurement.NotFound",
        message: $"Measurement with ID '{measurementId}' was not found.");
}
