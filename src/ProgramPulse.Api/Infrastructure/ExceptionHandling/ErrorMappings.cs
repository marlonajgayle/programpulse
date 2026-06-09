using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using ProgramPulse.Api.SharedKernel.Exceptions;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;

namespace ProgramPulse.Api.Infrastructure.ExceptionHandling;

public static class ErrorMappings
{
    private const string UnexpectedErrorDetail =
        "An unexpected error occurred. Please contact support if the problem persists.";

    public static ProblemDetails ToProblemDetails(
        this Exception exception,
        HttpContext context,
        bool isDevelopment)
    {
        var problemDetails = MapToError(exception, isDevelopment).ToProblemDetails();

        problemDetails.Instance = context.Request.Path;
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        return problemDetails;
    }

    private static Error MapToError(Exception exception, bool isDevelopment) =>
        exception switch
        {
            ValidationException ex => Error.Validation(
                "validation_failed",
                "One or more validation errors occurred.",
                new ValidationResult(ex.Errors).ToErrors()),

            DomainException ex => new Error(ex.Code, ex.Message, ex.Type),

            UnauthorizedAccessException => Error.Unauthorized(
                "unauthorized",
                "You are not authorized to access this resource."),

            _ => Error.Internal(
                "internal_server_error",
                isDevelopment ? exception.Message : UnexpectedErrorDetail)
        };
}
