using FluentValidation;
using ProgramPulse.Api.Domain.Authorization;

namespace ProgramPulse.Api.Features.Users.AddUser;

public sealed class AddUserCommandValidator : AbstractValidator<AddUserCommand>
{
    public AddUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("A first name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("A last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("An email address is required.")
            .EmailAddress().WithMessage("Invalid email format provided.");

        RuleFor(x => x.Roles)
            .NotEmpty().WithMessage("At least one role is required.");

        RuleForEach(x => x.Roles)
            .Must(role => Roles.GetAllRoles().Contains(role))
            .WithMessage("'{PropertyValue}' is not a valid role.");
    }
}
