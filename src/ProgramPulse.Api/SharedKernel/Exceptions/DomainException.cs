using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.SharedKernel.Exceptions;

// Base for exceptions that carry enough metadata to be mapped to a ProblemDetails
// using the same Error/ErrorType convention as the Result pipeline.
public abstract class DomainException : Exception
{
    protected DomainException(string code, string message, ErrorType type)
        : base(message)
    {
        Code = code;
        Type = type;
    }

    public string Code { get; }

    public ErrorType Type { get; }
}
