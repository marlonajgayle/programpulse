using Microsoft.AspNetCore.Identity;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Messaging.Outbox;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Authentication.ChangePassword;

public sealed record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword);

/// <summary>
/// Changes the authenticated caller's password. The caller is resolved from the current
/// request's JWT claims; ASP.NET Identity verifies the current password and applies the new one.
/// A failed change (incorrect current password or a password that violates Identity's policy) is
/// returned as a validation error carrying Identity's own description. On success a notification
/// email is enqueued into the outbox.
/// </summary>
public sealed class ChangePasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUser currentUser,
    IOutboxPublisher outboxPublisher,
    IApplicationDbContext dbContext,
    ILogger<ChangePasswordCommandHandler> logger)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IOutboxPublisher _outboxPublisher = outboxPublisher;
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<ChangePasswordCommandHandler> _logger = logger;

    public async Task<Result<string>> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            _logger.LogWarning("Change password failed: the caller is not authenticated.");
            return Result<string>.Failure(AuthenticationErrors.UserNotAuthenticated());
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            _logger.LogWarning("Change password failed: no user found with ID {UserId}.", userId);
            return Result<string>.Failure(AuthenticationErrors.UserNotFound(userId));
        }

        var changeResult = await _userManager.ChangePasswordAsync(
            user, command.CurrentPassword, command.NewPassword);

        if (!changeResult.Succeeded)
        {
            var description = changeResult.Errors.Select(e => e.Description).FirstOrDefault()
                ?? "Password change failed.";
            _logger.LogWarning(
                "Change password failed for user {UserId}: {Error}", userId, description);
            return Result<string>.Failure(AuthenticationErrors.PasswordChangeFailed(description));
        }

        _outboxPublisher.Add(
            nameof(PasswordChangedEmailRequestedEvent),
            new PasswordChangedEmailRequestedEvent(
                user.Email!,
                user.FirstName ?? "User",
                DateTime.UtcNow));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Password changed successfully.");
    }
}
