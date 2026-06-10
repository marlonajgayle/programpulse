namespace ProgramPulse.Api.Domain.Authorization;

/// <summary>
/// Canonical application role names and helpers used for Identity role seeding.
/// </summary>
public static class Roles
{
    public const string Administrator = "Administrator";
    public const string User = "User";

    public static IEnumerable<string> GetAllRoles() =>
    [
        Administrator,
        User
    ];
}
