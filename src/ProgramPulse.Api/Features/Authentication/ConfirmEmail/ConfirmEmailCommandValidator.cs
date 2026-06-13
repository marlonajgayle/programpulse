using FluentValidation;

namespace ProgramPulse.Api.Features.Authentication.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("An email address is required.")
            .EmailAddress().WithMessage("Invalid email format provided.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("A confirmation token is required.");
    }
}
