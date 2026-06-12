using Microsoft.AspNetCore.Identity;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Authentication.Login;

public sealed record LoginCommand(string Email, string Password);

/// <summary>
/// Result of a successful login. Carries the access token returned to the caller plus the
/// rotating refresh token, which the endpoint writes to an HttpOnly cookie (it is never
/// returned in the response body).
/// </summary>
public sealed record LoginResult(
    AccessTokenResponse AccessToken,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

/// <summary>
/// Validates credentials and, on success, issues a JWT access token and persists a new
/// rotating refresh token. The same <see cref="AuthenticationErrors.InvalidCredentials"/>
/// error is returned whether the user is missing, unconfirmed, or the password is wrong,
/// to avoid leaking which accounts exist.
/// </summary>
public sealed class LoginCommandHandler(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    ITokenService tokenService,
    IApplicationDbContext dbContext,
    IConfiguration configuration)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly IConfiguration _configuration = configuration;

    public async Task<Result<LoginResult>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);

        if (user is null || !user.EmailConfirmed)
            return Result<LoginResult>.Failure(AuthenticationErrors.InvalidCredentials());

        var signInResult = await _signInManager.CheckPasswordSignInAsync(
            user, command.Password, lockoutOnFailure: true);

        if (signInResult.IsLockedOut)
            return Result<LoginResult>.Failure(AuthenticationErrors.AccountLocked());

        if (!signInResult.Succeeded)
            return Result<LoginResult>.Failure(AuthenticationErrors.InvalidCredentials());

        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _tokenService.GenerateAccessToken(user, roles);

        var refreshTokenValue = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(
            _configuration.GetValue("JwtSettings:RefreshTokenExpirationDays", 7));

        _dbContext.RefreshTokens.Add(RefreshToken.Create(
            accessToken.Jti, refreshTokenValue, refreshTokenExpiry, user.Id));

        // user is tracked by the same scoped ApplicationDbContext that UserManager loaded
        // it from, so this update is persisted by SaveChangesAsync below.
        user.LastLoginAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<LoginResult>.Success(
            new LoginResult(accessToken, refreshTokenValue, refreshTokenExpiry));
    }
}
