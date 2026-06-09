namespace ProgramPulse.Api.Infrastructure;

public static class EnvironmentExtensions
{
    /// <summary>
    /// The name of the developer-machine environment that should behave like Development.
    /// </summary>
    public const string LocalEnvironmentName = "Local";

    /// <summary>
    /// True when running in the built-in Development environment or the custom Local
    /// environment. Use this for dev-only conveniences (console logging, API docs,
    /// detailed error responses) that should also be available on a developer machine.
    /// </summary>
    public static bool IsDevelopmentOrLocal(this IHostEnvironment env) =>
        env.IsDevelopment() || env.IsEnvironment(LocalEnvironmentName);
}
