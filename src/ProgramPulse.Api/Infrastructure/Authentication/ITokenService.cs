using System.Security.Claims;
using ProgramPulse.Api.Domain.Entities.Users;

namespace ProgramPulse.Api.Infrastructure.Authentication;

/// <summary>
/// Issues JWT access tokens and opaque refresh tokens, and re-reads the principal
/// from an expired access token (used by the refresh flow).
/// </summary>
public interface ITokenService
{
    AccessTokenResponse GenerateAccessToken(ApplicationUser user, IList<string> roles);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
