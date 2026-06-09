namespace ProgramPulse.Api.Infrastructure.ExceptionHandling;

public static class ExceptionHandlingConfiguration
{
    public static IServiceCollection AddGlobalExceptionHandling(
        this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        return services;
    }

    public static IApplicationBuilder UseGlobalExceptionHandling(
        this IApplicationBuilder app)
    {
        app.UseExceptionHandler();

        return app;
    }
}
