using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Authentication.Refresh;

/// <summary>
/// Exchanges a valid (active) refresh token for a fresh access token and a new
/// refresh token. The presented refresh token is rotated: it is revoked and linked
/// to its replacement so reuse of a stolen token can be detected.
/// </summary>
public sealed class RefreshEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (
            UserManager<ApplicationUser> userManager,
            ITokenService tokenService,
            IAuthCookieService authCookieService,
            IApplicationDbContext dbContext,
            IConfiguration configuration,
            HttpRequest request,
            HttpResponse response,
            CancellationToken cancellationToken) =>
        {
            var presentedToken = authCookieService.GetRefreshTokenFromCookie(request);

            if (string.IsNullOrEmpty(presentedToken))
                return Result<AccessTokenResponse>.Failure(AuthenticationErrors.InvalidRefreshToken()).ToHttpResult();

            var existing = await dbContext.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == presentedToken, cancellationToken);

            if (existing is null || !existing.IsActive)
                return Result<AccessTokenResponse>.Failure(AuthenticationErrors.InvalidRefreshToken()).ToHttpResult();

            var user = existing.User;
            var roles = await userManager.GetRolesAsync(user);
            var accessToken = tokenService.GenerateAccessToken(user, roles);

            var newRefreshTokenValue = tokenService.GenerateRefreshToken();
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(
                configuration.GetValue("JwtSettings:RefreshTokenExpirationDays", 7));

            existing.Revoke(reason: "Replaced by rotation", replacedByToken: newRefreshTokenValue);
            dbContext.RefreshTokens.Add(RefreshToken.Create(
                accessToken.Jti, newRefreshTokenValue, newRefreshTokenExpiry, user.Id));

            await dbContext.SaveChangesAsync(cancellationToken);

            authCookieService.SetAccessTokenCookie(response, accessToken.AccessToken, accessToken.ExpiresAt);
            authCookieService.SetRefreshTokenCookie(response, newRefreshTokenValue, newRefreshTokenExpiry);

            return Result<AccessTokenResponse>.Success(accessToken).ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithName("RefreshToken")
        .WithTags("Authentication");
    }
}
