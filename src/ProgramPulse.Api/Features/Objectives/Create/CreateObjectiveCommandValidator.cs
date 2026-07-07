using FluentValidation;

namespace ProgramPulse.Api.Features.Objectives.Create;

public sealed class CreateObjectiveCommandValidator : AbstractValidator<CreateObjectiveCommand>
{
    public CreateObjectiveCommandValidator()
    {
        // ProgrammeId is supplied from the route, not the request body.
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("A name is required.")
            .MaximumLength(200).WithMessage("The name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("A description is required.")
            .MaximumLength(1000).WithMessage("The description must not exceed 1000 characters.");

        RuleFor(x => x.Kpi)
            .NotNull().WithMessage("A KPI is required.")
            .SetValidator(new CreateObjectiveKpiValidator());
    }
}

public sealed class CreateObjectiveKpiValidator : AbstractValidator<CreateObjectiveKpi>
{
    public CreateObjectiveKpiValidator()
    {
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
    }
}
