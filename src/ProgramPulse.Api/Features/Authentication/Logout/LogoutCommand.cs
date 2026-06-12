using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Authentication.Logout;

public sealed record LogoutCommand(string? RefreshToken);

/// <summary>
/// Revokes the presented refresh token (if it exists and is still active) so it can no
/// longer be used to mint access tokens. Best-effort and idempotent: a missing, expired,
/// or already-revoked token is not an error. Cookie clearing is handled by the endpoint.
/// </summary>
public sealed class LogoutCommandHandler(IApplicationDbContext dbContext)
{
    private readonly IApplicationDbContext _dbContext = dbContext;

    public async Task<Result> HandleAsync(
        LogoutCommand command,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(command.RefreshToken))
        {
            var existing = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == command.RefreshToken, cancellationToken);

            if (existing is { IsActive: true })
            {
                existing.Revoke(reason: "User logout");
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        return Result.Success();
    }
}
