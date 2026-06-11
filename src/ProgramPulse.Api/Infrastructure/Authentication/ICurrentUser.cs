namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Exposes the authenticated caller derived from the current request's JWT
/// claims. Outside an HTTP request (migrations, seeding, background work)
/// <see cref="IsAuthenticated"/> is <c>false</c> and the claim values are
/// <c>null</c>.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    string? UserId { get; }
    string? Email { get; }
    string? FirstName { get; }
    string? LastName { get; }

    IReadOnlyCollection<string> Roles { get; }

    bool IsInRole(string role);
}
