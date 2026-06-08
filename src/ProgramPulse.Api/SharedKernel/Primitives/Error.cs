namespace ProgramPulse.Api.SharedKernel.Primitives;

public record Error
{
    public static readonly Error None = new (string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new ("null_value", "A required value was null.",ErrorType.Failure);

    public Error(
        string code, 
        string message,  
        ErrorType type,
        IReadOnlyDictionary<string, string[]>? details = null)
    {
        Code = code;
        Message = message;
        Type = type;
        Details = details;
    }

    public string Code {get;}
    public string Message { get; }
    public ErrorType Type { get; }
    public IReadOnlyDictionary<string, string[]>? Details { get; }

    public static Error Validation(string code, string message, IReadOnlyDictionary<string, string[]>? details = null) =>
        new (code, message, ErrorType.Validation, details);
    
    public static Error NotFound(string code, string message) =>
        new (code, message,  ErrorType.NotFound);

    public static Error Conflict(string code, string message) =>
        new (code, message,  ErrorType.Conflict);

    public static Error Unauthorized(string code, string message) =>
        new (code, message,  ErrorType.Unauthorized);

    public static Error Forbidden(string code, string message) =>
        new (code, message,  ErrorType.Forbidden);

    public static Error Internal(string code, string message) =>
        new (code, message,  ErrorType.Internal);

    
}

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized,
    Forbidden,
    Internal
}