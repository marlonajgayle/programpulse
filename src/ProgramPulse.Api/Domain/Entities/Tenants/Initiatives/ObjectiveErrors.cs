using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

public static class ObjectiveErrors
{
    public static Error ObjectiveNotFound(Guid objectiveId) => Error.NotFound(
        code: "Objective.NotFound",
        message: $"Objective with ID '{objectiveId}' was not found.");
}
