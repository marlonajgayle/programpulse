using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Users;

public static class RegistrationErrors
{
    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        code: "Registration.EmailAlreadyExists",
        message: $"An account with email '{email}' already exists.");

    public static Error UserCreationFailed(string message) => Error.Validation(
        code: "Registration.UserCreationFailed",
        message: message);
}
