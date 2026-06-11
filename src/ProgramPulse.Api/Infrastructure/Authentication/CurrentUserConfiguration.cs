namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Registers <see cref="IHttpContextAccessor"/> and the scoped
/// <see cref="ICurrentUser"/> implementation that reads the caller from the
/// current request's JWT claims.
/// </summary>
public static class CurrentUserConfiguration
{
    public static IServiceCollection AddCurrentUserService(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();

        return services;
    }
}
