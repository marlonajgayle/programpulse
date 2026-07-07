using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Programmes;

public static class ProgrammeErrors
{
    public static Error ProgrammeNotFound(Guid programmeId) => Error.NotFound(
        code: "Programme.NotFound",
        message: $"Programme with ID '{programmeId}' was not found.");
}
