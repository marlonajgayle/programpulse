using FluentValidation;

namespace ProgramPulse.Api.Features.Measurements.Create;

public sealed class CreateMeasurementCommandValidator : AbstractValidator<CreateMeasurementCommand>
{
    public CreateMeasurementCommandValidator()
    {
        // KpiId is supplied from the route, not the request body.
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("The notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);
    }
}
