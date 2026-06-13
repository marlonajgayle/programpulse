using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Users;

public static class UserManagementErrors
{
    public static Error EmailAlreadyExists(string email) => Error.Conflict(
        code: "UserManagement.EmailAlreadyExists",
        message: $"An account with email '{email}' already exists.");

    public static Error UserCreationFailed(string message) => Error.Validation(
        code: "UserManagement.UserCreationFailed",
        message: message);

    public static Error ActingAdminHasNoTenant() => Error.Internal(
        code: "UserManagement.ActingAdminHasNoTenant",
        message: "The acting administrator is not associated with a tenant.");
}
