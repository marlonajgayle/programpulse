using Microsoft.AspNetCore.Diagnostics;

namespace ProgramPulse.Api.Infrastructure.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IHostEnvironment _environment = environment;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}",
            context.TraceIdentifier);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "The response has already started, the global exception handler will not be executed.");
            return false;
        }

        var problemDetails = exception.ToProblemDetails(
            context,
            _environment.IsDevelopmentOrLocal());

        context.Response.StatusCode =
            problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(
            problemDetails,
            options: null,
            contentType: "application/problem+json",
            cancellationToken);

        return true;
    }
}
