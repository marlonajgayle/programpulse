using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Authentication;
using ProgramPulse.Api.Infrastructure.Messaging.Outbox;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Users.AddUser;

public sealed record AddUserCommand(
    string FirstName,
    string LastName,
    string Email,
    IReadOnlyList<string> Roles);

public sealed record AddUserResponse(string UserId);

/// <summary>
/// Lets a tenant administrator provision a new user inside their own tenant. The
/// user is created with a securely generated temporary password (auto-confirmed so
/// they can sign in immediately), assigned the requested roles, and emailed the
/// temporary password via the outbox. The whole operation runs in a single
/// transaction so a partial failure never leaves a user without roles or an
/// undelivered notification committed without the user.
/// </summary>
public sealed class AddUserCommandHandler(
    UserManager<ApplicationUser> userManager,
    ICurrentUser currentUser,
    IApplicationDbContext dbContext,
    IOutboxPublisher outboxPublisher,
    ILogger<AddUserCommandHandler> logger)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly IOutboxPublisher _outboxPublisher = outboxPublisher;
    private readonly ILogger<AddUserCommandHandler> _logger = logger;

    public async Task<Result<AddUserResponse>> HandleAsync(
        AddUserCommand command,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId;
        if (adminId is null)
        {
            return Result<AddUserResponse>.Failure(
                AuthenticationErrors.UserNotAuthenticated());
        }

        var admin = await _userManager.FindByIdAsync(adminId);
        if (admin?.TenantId is null)
        {
            _logger.LogWarning(
                "Add user failed: acting admin {AdminId} has no tenant.", adminId);
            return Result<AddUserResponse>.Failure(
                UserManagementErrors.ActingAdminHasNoTenant());
        }

        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
        {
            return Result<AddUserResponse>.Failure(
                UserManagementErrors.EmailAlreadyExists(command.Email));
        }

        var temporaryPassword = GenerateTemporaryPassword();

        var strategy = _dbContext.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
            try
            {
                var user = new ApplicationUser
                {
                    UserName = command.Email,
                    Email = command.Email,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    TenantId = admin.TenantId,
                    EmailConfirmed = true
                };

                var created = await _userManager.CreateAsync(user, temporaryPassword);
                if (!created.Succeeded)
                {
                    await _dbContext.RollbackTransactionAsync(transaction, cancellationToken);
                    var message = string.Join("; ", created.Errors.Select(e => e.Description));

                    return Result<AddUserResponse>.Failure(
                        UserManagementErrors.UserCreationFailed(message));
                }

                await _userManager.AddToRolesAsync(user, command.Roles);

                _outboxPublisher.Add(
                    nameof(NewUserWelcomeEmailRequestedEvent),
                    new NewUserWelcomeEmailRequestedEvent(
                        user.Email!,
                        user.FirstName ?? user.Email!,
                        temporaryPassword,
                        DateTime.UtcNow));

                await _dbContext.SaveChangesAsync(cancellationToken);
                await _dbContext.CommitTransactionAsync(transaction, cancellationToken);

                _logger.LogInformation(
                    "Admin {AdminId} added user {UserId} to tenant {TenantId}.",
                    adminId, user.Id, admin.TenantId);

                return Result<AddUserResponse>.Created(
                    new AddUserResponse(user.Id),
                    $"/api/v1/users/{user.Id}");
            }
            catch
            {
                await _dbContext.RollbackTransactionAsync(transaction, cancellationToken);
                throw;
            }
        });
    }

    /// <summary>
    /// Produces a cryptographically random temporary password that satisfies the
    /// Identity password policy: at least one uppercase, lowercase, digit and
    /// non-alphanumeric character.
    /// </summary>
    private static string GenerateTemporaryPassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*?-_";
        const string all = upper + lower + digits + symbols;

        var chars = new List<char>
        {
            upper[RandomNumberGenerator.GetInt32(upper.Length)],
            lower[RandomNumberGenerator.GetInt32(lower.Length)],
            digits[RandomNumberGenerator.GetInt32(digits.Length)],
            symbols[RandomNumberGenerator.GetInt32(symbols.Length)]
        };

        while (chars.Count < 16)
        {
            chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);
        }

        // Fisher-Yates shuffle so the guaranteed characters aren't always first.
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string([.. chars]);
    }
}
