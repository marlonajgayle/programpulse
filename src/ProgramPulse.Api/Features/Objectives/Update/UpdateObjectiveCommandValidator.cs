using FluentValidation;

namespace ProgramPulse.Api.Features.Objectives.Update;

public sealed class UpdateObjectiveCommandValidator : AbstractValidator<UpdateObjectiveCommand>
{
    public UpdateObjectiveCommandValidator()
    {
        // Id is supplied from the route, not the request body.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("A name is required.")
            .MaximumLength(200).WithMessage("The name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A description is required.")
            .MaximumLength(1000).WithMessage("The description must not exceed 1000 characters.");
    }
}
