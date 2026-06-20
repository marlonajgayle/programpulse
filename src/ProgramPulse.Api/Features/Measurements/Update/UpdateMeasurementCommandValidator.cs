using FluentValidation;

namespace ProgramPulse.Api.Features.Measurements.Update;

public sealed class UpdateMeasurementCommandValidator : AbstractValidator<UpdateMeasurementCommand>
{
    public UpdateMeasurementCommandValidator()
    {
        // Id is supplied from the route, not the request body.
        RuleFor(x => x.Notes)
            .MaximumLength(1000).WithMessage("The notes must not exceed 1000 characters.")
            .When(x => x.Notes is not null);
    }
}
