using FluentValidation;

namespace ProgramPulse.Api.Features.Kpis.Create;

public sealed class CreateKpiCommandValidator : AbstractValidator<CreateKpiCommand>
{
    public CreateKpiCommandValidator()
    {
        // ObjectiveId is supplied from the route, not the request body.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("A KPI name is required.")
            .MaximumLength(200).WithMessage("The KPI name must not exceed 200 characters.");

        RuleFor(x => x.Unit)
            .NotEmpty().WithMessage("A KPI unit is required.")
            .MaximumLength(50).WithMessage("The KPI unit must not exceed 50 characters.");

        RuleFor(x => x.Direction)
            .IsInEnum().WithMessage("The KPI direction is invalid.");

        RuleFor(x => x.DueDate)
            .NotEmpty().WithMessage("A KPI due date is required.");

        RuleFor(x => x.Frequency)
            .IsInEnum().WithMessage("The measurement frequency is invalid.")
            .When(x => x.Frequency.HasValue);

        RuleFor(x => x.Strategies)
            .MaximumLength(2000).WithMessage("Strategies must not exceed 2000 characters.")
            .When(x => x.Strategies is not null);

        RuleFor(x => x.Activities)
            .MaximumLength(2000).WithMessage("Activities must not exceed 2000 characters.")
            .When(x => x.Activities is not null);

        RuleFor(x => x.KeyOutputs)
            .MaximumLength(2000).WithMessage("Key outputs must not exceed 2000 characters.")
            .When(x => x.KeyOutputs is not null);

        RuleFor(x => x.PerformanceMeasure)
            .MaximumLength(2000).WithMessage("Performance measure must not exceed 2000 characters.")
            .When(x => x.PerformanceMeasure is not null);
    }
}
