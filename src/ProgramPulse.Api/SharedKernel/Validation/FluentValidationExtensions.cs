using FluentValidation.Results;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.SharedKernel.Validation;

public static class FluentValidationExtensions
{
    public static Dictionary<string, string[]> ToErrors(
        this ValidationResult validationResult)
    {
        return validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());
    }

    // Bridge into the existing Result/Error pipeline.
    public static Error ToValidationError(this ValidationResult validationResult) =>
        Error.Validation(
            "validation_failed",
            "One or more validation errors occurred.",
            validationResult.ToErrors());
}
