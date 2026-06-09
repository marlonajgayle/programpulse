using Microsoft.AspNetCore.Mvc;

namespace ProgramPulse.Api.SharedKernel.Primitives;

// Single source of truth for turning an Error/ErrorType into the project's
// RFC 7807 ProblemDetails shape. Shared by the Result pipeline (ResultExtensions)
// and the global exception handler so every error path looks identical.
public static class ProblemDetailsMapping
{
    public static ProblemDetails ToProblemDetails(this Error error)
    {
        var problemDetails = new ProblemDetails
        {
            Status = GetStatusCode(error.Type),
            Title = GetTitle(error.Type),
            Type = GetTypeUri(error.Type),
            Detail = error.Message
        };

        problemDetails.Extensions["errors"] = error.Details is not null
            ? error.Details
            : new[] { new { error.Code, error.Message } };

        return problemDetails;
    }

    public static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

    public static string GetTitle(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound => "Resource Not Found",
            ErrorType.Validation => "Validation Error",
            ErrorType.Conflict => "Conflict Error",
            ErrorType.Unauthorized => "Unauthorized Access",
            ErrorType.Forbidden => "Forbidden Access",
            _ => "Internal Server Error"
        };

    public static string GetTypeUri(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound => "https://httpstatuses.com/404",
            ErrorType.Validation => "https://httpstatuses.com/400",
            ErrorType.Conflict => "https://httpstatuses.com/409",
            ErrorType.Unauthorized => "https://httpstatuses.com/401",
            ErrorType.Forbidden => "https://httpstatuses.com/403",
            _ => "https://httpstatuses.com/500"
        };
}
