using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using ProgramPulse.Api.Domain.Entities.Users;
using ProgramPulse.Api.SharedKernel.Primitives;

namespace ProgramPulse.Api.Features.Authentication.ResetPassword;

public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword);

/// <summary>
/// Completes a password reset. The token is the Base64Url-encoded ASP.NET Identity reset
/// token delivered by email (see <c>ForgotPasswordCommandHandler</c>); it is decoded back to
/// the original token before <see cref="UserManager{TUser}.ResetPasswordAsync"/> validates it.
/// A failed reset (invalid/expired token or a password that violates Identity's policy) is
/// returned as a validation error carrying Identity's own description.
/// </summary>
public sealed class ResetPasswordCommandHandler(
    UserManager<ApplicationUser> userManager,
    ILogger<ResetPasswordCommandHandler> logger)
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<ResetPasswordCommandHandler> _logger = logger;

    public async Task<Result<string>> HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(command.Email);
        if (user is null)
            return Result<string>.Failure(AuthenticationErrors.UserNotFound(command.Email));

        string decodedToken;
        try
        {
            decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(command.Token));
        }
        catch (FormatException)
        {
            _logger.LogWarning(
                "Malformed password reset token supplied for email: {Email}", command.Email);
            return Result<string>.Failure(
                AuthenticationErrors.PasswordResetFailed("The password reset token is invalid."));
        }

        var resetResult = await _userManager.ResetPasswordAsync(
            user, decodedToken, command.NewPassword);

        if (!resetResult.Succeeded)
        {
            var description = resetResult.Errors.Select(e => e.Description).FirstOrDefault()
                ?? "Password reset failed.";
            _logger.LogWarning(
                "Password reset failed for email {Email}: {Error}", command.Email, description);
            return Result<string>.Failure(AuthenticationErrors.PasswordResetFailed(description));
        }

        return Result<string>.Success("Password has been reset successfully.");
    }
}
