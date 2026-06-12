using ProgramPulse.Api.Infrastructure.Authentication;
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
            LoginCommand command,
            LoginCommandHandler handler,
            IAuthCookieService authCookieService,
            HttpResponse response,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(command, cancellationToken);

            if (!result.IsSuccess)
                return result.ToHttpResult();

            var login = result.Value!;

            authCookieService.SetAccessTokenCookie(
                response, login.AccessToken.AccessToken, login.AccessToken.ExpiresAt);
            authCookieService.SetRefreshTokenCookie(
                response, login.RefreshToken, login.RefreshTokenExpiresAt);

            return Result<AccessTokenResponse>.Success(login.AccessToken).ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .WithValidation<LoginCommand>()
        .WithName("Login")
        .WithTags("Authentication");
    }
}
