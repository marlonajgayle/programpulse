using FluentValidation;

namespace ProgramPulse.Api.Features.Authentication.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty().WithMessage("An organization name is required.")
            .MaximumLength(200).WithMessage("Organization name must not exceed 200 characters.")
            .Must(name => name.Any(char.IsLetterOrDigit))
            .WithMessage("Organization name must contain at least one letter or digit.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("A first name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("A last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("An email address is required.")
            .EmailAddress().WithMessage("Invalid email format provided.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one non-alphanumeric character.");
    }
}
