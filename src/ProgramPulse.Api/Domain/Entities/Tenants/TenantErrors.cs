using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Tenants;

public static class TenantErrors
{
    public static Error TenantNotFound(Guid tenantId) => Error.NotFound(
        code: "Tenant.NotFound",
        message: $"Tenant with ID '{tenantId}' was not found.");

    public static Error SlugAlreadyExists(string slug) => Error.Conflict(
        code: "Tenant.SlugAlreadyExists",
        message: $"A tenant with the slug '{slug}' already exists.");

    public static Error Validation(
        Dictionary<string, string[]> errors) =>
        Error.Validation(
            code: "Tenant.Validation_Error",
            message: "One or more validation errors occurred.",
            errors);

    public static Error Failure(string message) => Error.Internal(
        code: "Tenant.Failed",
        message: message);
}
