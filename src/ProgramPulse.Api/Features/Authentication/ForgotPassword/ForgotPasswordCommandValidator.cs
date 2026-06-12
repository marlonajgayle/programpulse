using FluentValidation;

namespace ProgramPulse.Api.Features.Authentication.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("An email address is required.")
            .EmailAddress().WithMessage("Invalid email format provided.");
    }
}
