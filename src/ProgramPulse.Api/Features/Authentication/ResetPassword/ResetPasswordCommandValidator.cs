using FluentValidation;

namespace ProgramPulse.Api.Features.Authentication.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("An email address is required.")
            .EmailAddress().WithMessage("Invalid email format provided.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("A reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("A new password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
