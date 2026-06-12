using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.SharedKernel;
using ProgramPulse.Api.SharedKernel.Primitives;
using ProgramPulse.Api.SharedKernel.Versioning;

namespace ProgramPulse.Api.Features.Authentication.Logout;

/// <summary>
/// Logs the current user out: revokes the presented refresh token (if any) and clears
/// both auth cookies. Requires an authenticated caller.
/// </summary>
public sealed class LogoutEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (
            LogoutCommandHandler handler,
            IAuthCookieService authCookieService,
            HttpRequest request,
            HttpResponse response,
            CancellationToken cancellationToken) =>
        {
            var presentedToken = authCookieService.GetRefreshTokenFromCookie(request);

            var result = await handler.HandleAsync(new LogoutCommand(presentedToken), cancellationToken);

            authCookieService.ClearAuthCookies(response);

            return result.ToHttpResult();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("Logout")
        .WithTags("Authentication");
    }
}
