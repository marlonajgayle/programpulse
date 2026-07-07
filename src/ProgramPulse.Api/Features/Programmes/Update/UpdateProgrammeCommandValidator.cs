using FluentValidation;

namespace ProgramPulse.Api.Features.Initiatives.Update;

public sealed class UpdateInitiativeCommandValidator : AbstractValidator<UpdateInitiativeCommand>
{
    public UpdateInitiativeCommandValidator()
    {
        // Id is supplied from the route, not the request body.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("A name is required.")
            .MaximumLength(200).WithMessage("The name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A description is required.")
            .MaximumLength(1000).WithMessage("The description must not exceed 1000 characters.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("The end date cannot be earlier than the start date.");
    }
}
