namespace ProgramPulse.Api.SharedKernel.Primitives;

public abstract class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot have an error.");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failure result must have an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() =>
        new SuccessResult();

    public static Result Failure(Error error) =>
        new FailureResult(error);

    private sealed class SuccessResult : Result
    {
        public SuccessResult()
            : base(true, Error.None) { }
    }

    private sealed class FailureResult : Result
    {
        public FailureResult(Error error)
            : base(false, error) { }
    }
}
