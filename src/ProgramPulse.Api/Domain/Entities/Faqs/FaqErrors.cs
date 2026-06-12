using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Domain.Entities.Faqs;

public static class FaqErrors
{
    public static Error FaqNotFound(Guid faqId) => Error.NotFound(
        code: "Faq.NotFound",
        message: $"FAQ with ID '{faqId}' was not found.");

    public static Error Validation(
        Dictionary<string, string[]> errors) =>
        Error.Validation(
            code: "Faq.Validation_Error",
            message: "One or more validation errors occurred.",
            errors);

    public static Error Failure(string message) => Error.Internal(
        code: "Faq.Failed",
        message: message);
}
