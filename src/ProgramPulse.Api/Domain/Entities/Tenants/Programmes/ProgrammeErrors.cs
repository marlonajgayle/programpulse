using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants.Initiatives;

public static class InitiativeErrors
{
    public static Error InitiativeNotFound(Guid initiativeId) => Error.NotFound(
        code: "Initiative.NotFound",
        message: $"Initiative with ID '{initiativeId}' was not found.");
}
