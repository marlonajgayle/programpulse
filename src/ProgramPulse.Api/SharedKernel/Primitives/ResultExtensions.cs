namespace ProgramPulse.Api.SharedKernel.Primitives;

public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsFailure)
        {
            if (result.Error.Details is not null)
            {
                return Results.Problem(
                    statusCode: GetStatusCode(result.Error.Type),
                    title: GetTitle(result.Error.Type),
                    type: GetTypeUri(result.Error.Type),
                    detail: result.Error.Message,
                    extensions: new Dictionary<string, object?>
                    {
                        {"errors", result.Error.Details}
                    });
            }

            return Results.Problem(
                statusCode: GetStatusCode(result.Error.Type),
                title: GetTitle(result.Error.Type),
                type: GetTypeUri(result.Error.Type),
                detail: result.Error.Message,
                extensions: new Dictionary<string, object?>
                {
                    {"errors", new[] {new {result.Error.Code, result.Error.Message}}}
                });
        }
            
        return Results.NoContent();
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsFailure)
        {
            if ( result.Error.Details is not null)
            {
                return Results.Problem(
                    statusCode: GetStatusCode(result.Error.Type),
                    title: GetTitle(result.Error.Type),
                    type: GetTypeUri(result.Error.Type),
                    detail: result.Error.Message,
                    extensions: new Dictionary<string, object?>
                    {
                        {"errors", result.Error.Details}
                    });
            }

            return Results.Problem(
                statusCode: GetStatusCode(result.Error.Type),
                title: GetTitle(result.Error.Type),
                type: GetTypeUri(result.Error.Type),
                detail: result.Error.Message,
                extensions: new Dictionary<string, object?>
                {
                    {"errors", new[] {new {result.Error.Code, result.Error.Message}}}
                });
        }

        if (result.IsAccepted)
            return Results.Accepted(result.Location, result.Value);

        if (result.Location is not null)
            return Results.Created(result.Location, result.Value);

        return Results.Ok(result.Value);
    }

    private static int GetStatusCode(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError
        };

    private static string? GetTitle(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound => "Resource Not Found",
            ErrorType.Validation => "Validation Error",
            ErrorType.Conflict => "Conflict Error",
            ErrorType.Unauthorized => "Unauthorized Access",
            ErrorType.Forbidden => "Forbidden Access",
            _ => "Internal Server Error"
        };

    private static string GetTypeUri(ErrorType errorType) =>
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