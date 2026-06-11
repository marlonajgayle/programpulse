using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Validation;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Authentication.Login;

/// <summary>
/// Authenticates a user with email + password and issues a JWT access token plus a
/// rotating refresh token. Both tokens are written to HttpOnly cookies; the access
/// token is also returned in the response body for header-based clients.
/// </summary>
public sealed class LoginEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            LoginRequest request,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ITokenService tokenService,
            IAuthCookieService authCookieService,
            IApplicationDbContext dbContext,
            IConfiguration configuration,
            HttpResponse response,
            CancellationToken cancellationToken) =>
        {
            var user = await userManager.FindByEmailAsync(request.Email);

            // Return the same error whether the user is missing, unconfirmed, or the
            // password is wrong, to avoid leaking which accounts exist.
            if (user is null || !user.EmailConfirmed)
                return Result<AccessTokenResponse>.Failure(AuthenticationErrors.InvalidCredentials()).ToHttpResult();

            var signInResult = await signInManager.CheckPasswordSignInAsync(
                user, request.Password, lockoutOnFailure: true);

            if (signInResult.IsLockedOut)
                return Result<AccessTokenResponse>.Failure(AuthenticationErrors.AccountLocked()).ToHttpResult();

            if (!signInResult.Succeeded)
                return Result<AccessTokenResponse>.Failure(AuthenticationErrors.InvalidCredentials()).ToHttpResult();

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = tokenService.GenerateAccessToken(user, roles);

            var refreshTokenValue = tokenService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                configuration.GetValue("JwtSettings:RefreshTokenExpirationDays", 7));

            dbContext.RefreshTokens.Add(RefreshToken.Create(
                accessToken.Jti, refreshTokenValue, refreshTokenExpiry, user.Id));

            // user is tracked by the same scoped ApplicationDbContext that UserManager
            // loaded it from, so this update is persisted by SaveChangesAsync below.
            user.LastLoginAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync(cancellationToken);

            authCookieService.SetAccessTokenCookie(response, accessToken.AccessToken, accessToken.ExpiresAt);
            authCookieService.SetRefreshTokenCookie(response, refreshTokenValue, refreshTokenExpiry);

            return Result<AccessTokenResponse>.Success(accessToken).ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<LoginRequest>()
        .WithName("Login")
        .WithTags("Authentication");
    }
}

public sealed record LoginRequest(string Email, string Password);
