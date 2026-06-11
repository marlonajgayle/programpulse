namespace ProgramPulse.Api.Domain.Authorization;

/// <summary>
/// Named authorization policies and their registration. Scoped to the roles the
/// application currently defines (<see cref="Roles.Administrator"/> and
/// <see cref="Roles.User"/>); add more policies here as new roles land.
/// </summary>
public static class AuthorizationPolicies
{
    public const string AdminOnly = nameof(AdminOnly);
    public const string Authenticated = nameof(Authenticated);

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(AdminOnly, policy =>
                policy.RequireRole(Roles.Administrator))
            .AddPolicy(Authenticated, policy =>
                policy.RequireAuthenticatedUser());

        return services;
    }
}
