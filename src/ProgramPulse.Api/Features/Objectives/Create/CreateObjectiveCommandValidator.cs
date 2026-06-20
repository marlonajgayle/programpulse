using FluentValidation;

namespace ProgramPulse.Api.Features.Objectives.Create;

public sealed class CreateObjectiveCommandValidator : AbstractValidator<CreateObjectiveCommand>
{
    public CreateObjectiveCommandValidator()
    {
        // InitiativeId is supplied from the route, not the request body.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("A name is required.")
            .MaximumLength(200).WithMessage("The name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A description is required.")
            .MaximumLength(1000).WithMessage("The description must not exceed 1000 characters.");
    }
}
