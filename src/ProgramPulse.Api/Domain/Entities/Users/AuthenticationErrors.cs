using ProgramPulse.Api.SharedKernel.Primitives;

namespace VitalDonor.Api.Domain.Entities.Authentications;

public static class AuthenticationErrors
{
    public static Error InvalidCredentials() =>
        Error.Unauthorized(
            code: "Authentication.InvalidCredentials",
            message: "The provided credentials are invalid."
        );

    public static Error UserNotAuthenticated() =>
        Error.Unauthorized(
            code: "Authentication.UserNotAuthenticated",
            message: "The user is not authenticated."
        );

    public static Error Validation(
        Dictionary<string, string[]> errors) =>
        Error.Validation(
            code: "Authentication.Validation_Error",
            message: "One or more validation errors occurred.",
            errors
        );

    public static Error UserNotFound(string email) =>
        Error.NotFound(
            code: "Authentication.UserNotFound",
            message: $"No user found with email: {email}."
        );

    public static Error Failure(string message) =>
        Error.Internal(
            code: "Authentication.Failure",
            message: message
        );

    public static Error InvalidTwoFactorCode() =>
        Error.Unauthorized(
            code: "Authentication.InvalidTwoFactorCode",
            message: "The provided two-factor authentication code is invalid."
        );
    
}