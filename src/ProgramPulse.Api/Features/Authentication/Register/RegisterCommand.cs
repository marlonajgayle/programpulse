using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProgramPulse.Api.Domain.Authorization;
using ProgramPulse.Api.Domain.Entities.Tenants;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Messaging.Outbox;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Authentication.Register;

public sealed record RegisterCommand(
    string TenantName,
    string FirstName,
    string LastName,
    string Email,
    string Password);

public sealed record RegisterResponse(Guid TenantId, string UserId);

/// <summary>
/// Onboards a new organization: creates a <see cref="Tenant"/> and its first administrator
/// in a single database transaction so a partial failure never leaves an orphaned tenant or
/// a user without a tenant. The admin's email is auto-confirmed so they can sign in
/// immediately, and a welcome email is enqueued into the outbox (delivered only after commit).
/// </summary>
public sealed class RegisterCommandHandler(
    UserManager<ApplicationUser> userManager,
    IApplicationDbContext dbContext,
    IOutboxPublisher outboxPublisher,
    ILogger<RegisterCommandHandler> logger)
{
    private static readonly Regex NonAlphanumericRuns =
        new("[^a-z0-9]+", RegexOptions.Compiled);

    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly IOutboxPublisher _outboxPublisher = outboxPublisher;
    private readonly ILogger<RegisterCommandHandler> _logger = logger;

    public async Task<Result<RegisterResponse>> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(command.Email);
        if (existingUser is not null)
        {
            return Result<RegisterResponse>.Failure(
                RegistrationErrors.EmailAlreadyExists(command.Email));
        }

        var slug = GenerateSlug(command.TenantName);
        var slugTaken = await _dbContext.Tenants
            .AnyAsync(t => t.Slug == slug, cancellationToken);
        if (slugTaken)
        {
            return Result<RegisterResponse>.Failure(TenantErrors.SlugAlreadyExists(slug));
        }

        var strategy = _dbContext.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            var transaction = await _dbContext.BeginTransactionAsync(cancellationToken);
            try
            {
                var tenant = Tenant.Create(
                    command.TenantName, 
                    slug, 
                    description: null);

                _dbContext.Tenants.Add(tenant);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var user = new ApplicationUser
                {
                    UserName = command.Email,
                    Email = command.Email,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    TenantId = tenant.Id,
                    EmailConfirmed = true
                };

                var created = await _userManager.CreateAsync(user, command.Password);
                if (!created.Succeeded)
                {
                    await _dbContext.RollbackTransactionAsync(transaction, cancellationToken);
                    var message = string.Join("; ", created.Errors.Select(e => e.Description));
                    
                    return Result<RegisterResponse>.Failure(
                        RegistrationErrors.UserCreationFailed(message));
                }

                await _userManager.AddToRoleAsync(user, Roles.Administrator);

                _outboxPublisher.Add(
                    nameof(WelcomeEmailRequestedEvent),
                    new WelcomeEmailRequestedEvent(
                        user.Email!,
                        user.FirstName ?? user.Email!,
                        DateTime.UtcNow));

                await _dbContext.SaveChangesAsync(cancellationToken);
                await _dbContext.CommitTransactionAsync(transaction, cancellationToken);

                _logger.LogInformation(
                    "Registered tenant {TenantId} with admin user {UserId}.",
                    tenant.Id, user.Id);

                return Result<RegisterResponse>.Created(
                    new RegisterResponse(tenant.Id, user.Id),
                    $"/api/v1/tenants/{tenant.Id}");
            }
            catch
            {
                await _dbContext.RollbackTransactionAsync(transaction, cancellationToken);
                throw;
            }
        });
    }

    /// <summary>
    /// Produces a URL-friendly slug from a tenant name: lowercased, with runs of
    /// non-alphanumeric characters collapsed to single hyphens and edges trimmed.
    /// </summary>
    private static string GenerateSlug(string name)
    {
        var slug = NonAlphanumericRuns.Replace(name.Trim().ToLowerInvariant(), "-");
        return slug.Trim('-');
    }
}
