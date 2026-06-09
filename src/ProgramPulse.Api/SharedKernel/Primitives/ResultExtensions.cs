namespace ProgramPulse.Api.SharedKernel.Primitives;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsFailure)
            return Results.Problem(result.Error.ToProblemDetails());

        return Results.NoContent();
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsFailure)
            return Results.Problem(result.Error.ToProblemDetails());

        if (result.IsAccepted)
            return Results.Accepted(result.Location, result.Value);

        if (result.Location is not null)
            return Results.Created(result.Location, result.Value);

        return Results.Ok(result.Value);
    }
}
