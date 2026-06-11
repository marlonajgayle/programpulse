using System.Security.Claims;

namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Reads the current <see cref="ClaimsPrincipal"/> from
/// <see cref="IHttpContextAccessor"/> and projects JWT claims onto
/// <see cref="ICurrentUser"/>. Each accessor prefers the framework
/// <see cref="ClaimTypes"/> mapping and falls back to the raw JWT claim name to
/// support both mapped and unmapped token handlers.
/// </summary>
public class CurrentUserService(
    IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated ?? false;

    public string? UserId =>
        GetClaimValue(ClaimTypes.NameIdentifier) ??
        GetClaimValue("sub");

    public string? Email =>
        GetClaimValue(ClaimTypes.Email) ??
        GetClaimValue("email");

    public string? FirstName =>
        GetClaimValue(ClaimTypes.GivenName) ??
        GetClaimValue("given_name");

    public string? LastName =>
        GetClaimValue(ClaimTypes.Surname) ??
        GetClaimValue("family_name");

    public IReadOnlyCollection<string> Roles =>
        User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToArray()
        ?? [];

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    private string? GetClaimValue(string claimType) =>
        User?.FindFirst(claimType)?.Value;
}
