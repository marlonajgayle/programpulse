using FluentValidation;

namespace ProgramPulse.Api.Features.Kpis.Update;

public sealed class UpdateKpiCommandValidator : AbstractValidator<UpdateKpiCommand>
{
    public UpdateKpiCommandValidator()
    {
        // Id is supplied from the route, not the request body.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("A name is required.")
            .MaximumLength(200).WithMessage("The name must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("A unit is required.")
            .MaximumLength(50).WithMessage("The unit must not exceed 50 characters.");

        RuleFor(x => x.Direction)
            .IsInEnum().WithMessage("The direction is invalid.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("A due date is required.");
    }
}
