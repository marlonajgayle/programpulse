using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Authentication.ConfirmEmail;

public sealed record ConfirmEmailCommand(string Email, string Token);

/// <summary>
/// Completes email verification by validating the ASP.NET Identity confirmation token carried
/// in a confirmation link and marking the account confirmed. Re-clicking the link on an
/// already-confirmed account is idempotent and returns success without re-validating the token.
/// </summary>
public sealed class ConfirmEmailCommandHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<ConfirmEmailCommandHandler> logger)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<ConfirmEmailCommandHandler> _logger = logger;

    public async Task<Result<string>> HandleAsync(
        ConfirmEmailCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
        {
            _logger.LogWarning(
                "Email confirmation requested for non-existent email: {Email}", command.Email);
            return Result<string>.Failure(AuthenticationErrors.UserNotFound(command.Email));
        }

        if (user.EmailConfirmed)
        {
            return Result<string>.Success("Email already confirmed.");
        }

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Token));
        }
        catch (FormatException)
        {
            _logger.LogWarning(
                "Malformed email confirmation token supplied for email: {Email}", command.Email);
            return Result<string>.Failure(
                AuthenticationErrors.EmailConfirmationFailed("The confirmation token is invalid."));
        }

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded)
        {
            var description = string.Join("; ", result.Errors.Select(e => e.Description));
            _logger.LogWarning(
                "Email confirmation failed for email {Email}: {Error}", command.Email, description);
            return Result<string>.Failure(AuthenticationErrors.EmailConfirmationFailed(description));
        }

        return Result<string>.Success("Email confirmed successfully.");
    }
}
