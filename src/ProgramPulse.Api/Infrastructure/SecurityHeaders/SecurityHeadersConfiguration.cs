namespace ProgramPulse.Api.Infrastructure.SecurityHeaders;

public static class SecurityHeadersConfiguration
{
    public static IServiceCollection AddSecurityHeadersConfiguration(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SecurityHeadersOption>(
            configuration.GetSection(SecurityHeadersOption.SectionName));

        return services;
    }

    public static IApplicationBuilder UseSecurityHeadersConfiguration(
        this IApplicationBuilder app, IConfiguration configuration)
    {
        var options = configuration
            .GetSection(SecurityHeadersOption.SectionName)
            .Get<SecurityHeadersOption>() ?? new SecurityHeadersOption();

        if (options.Enabled)
        {
            app.UseMiddleware<SecurityHeadersMiddleware>();
        }

        return app;
    }
}
