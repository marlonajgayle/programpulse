using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.Features.Notifications.Events;
using ProgramPulse.Api.Infrastructure.Messaging.Outbox;
using ProgramPulse.Api.Infrastructure.Persistence;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Authentication.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email);

/// <summary>
/// Starts the password-reset flow. For a registered, email-confirmed user this generates
/// an ASP.NET Identity reset token and enqueues a reset email into the outbox. The same
/// generic message is returned in every case — including unknown or unconfirmed accounts —
/// so the endpoint never reveals which emails exist and never leaks the reset link.
/// </summary>
public sealed class ForgotPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    IHttpContextAccessor httpContextAccessor,
    IOutboxPublisher outboxPublisher,
    IApplicationDbContext dbContext,
    ILogger<ForgotPasswordCommandHandler> logger)
{
    private const string GenericMessage =
        "If the email is registered and confirmed, a password reset link will be sent.";

    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private readonly IOutboxPublisher _outboxPublisher = outboxPublisher;
    private readonly IApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger = logger;

    public async Task<Result<string>> HandleAsync(
        ForgotPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);

        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogWarning(
                "Password reset requested for non-existent or unconfirmed email: {Email}",
                command.Email);
            return Result<string>.Success(GenericMessage);
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

        var request = _httpContextAccessor.HttpContext!.Request;
        var resetLink = QueryHelpers.AddQueryString(
            $"{request.Scheme}://{request.Host}/reset-password",
            new Dictionary<string, string?>
            {
                ["email"] = command.Email,
                ["token"] = encodedToken
            });

        _outboxPublisher.Add(
            nameof(PasswordResetEmailRequestedEvent),
            new PasswordResetEmailRequestedEvent(
                user.Email!,
                user.FirstName ?? "User",
                resetLink,
                DateTime.UtcNow));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(GenericMessage);
    }
}
