namespace ProgramPulse.Api.SharedKernel.Primitives;

public sealed class Result<T> : Result
{
    public T? Value { get; }
    public string? Location { get; }
    public bool IsAccepted { get; }


    private Result(
        bool isSuccess, 
        T? value, 
        Error error, 
        string? location = null, 
        bool isAccepted = false)
        : base(isSuccess, error)
    {
        Value = value;
        Location = location;
        IsAccepted = isAccepted;
    }

    public static Result<T> Success(T value) =>
        new(true, value, Error.None);

    public static Result<T> Created(T value, string location) =>
        new(true, value, Error.None, location);

    public static Result<T> Accepted(T value, string? location) =>
        new(true, value, Error.None, location, isAccepted: true);

    public new static Result<T> Failure(Error error) =>
        new(false, default, error);
}