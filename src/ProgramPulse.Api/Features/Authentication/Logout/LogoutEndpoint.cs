using Asp.Versioning;
using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel;
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
            IAuthCookieService authCookieService,
            IApplicationDbContext dbContext,
            HttpRequest request,
            HttpResponse response,
            CancellationToken cancellationToken) =>
        {
            var presentedToken = authCookieService.GetRefreshTokenFromCookie(request);

            if (!string.IsNullOrEmpty(presentedToken))
            {
                var existing = await dbContext.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == presentedToken, cancellationToken);

                if (existing is { IsActive: true })
                {
                    existing.Revoke(reason: "User logout");
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            authCookieService.ClearAuthCookies(response);

            return Results.NoContent();
        })
        .HasApiVersion(ApiVersions.V1)
        .RequireAuthorization(AuthorizationPolicies.Authenticated)
        .WithName("Logout")
        .WithTags("Authentication");
    }
}
