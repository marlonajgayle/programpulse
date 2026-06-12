using FluentValidation;

namespace ProgramPulse.Api.Features.Authentication.Login;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("An email address is required.")
            .EmailAddress().WithMessage("Invalid email format provided.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A password is required.");
    }
}
