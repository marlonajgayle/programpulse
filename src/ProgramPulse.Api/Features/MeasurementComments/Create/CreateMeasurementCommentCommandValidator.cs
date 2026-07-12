using FluentValidation;

namespace ProgramPulse.Api.Features.MeasurementComments.Create;

public sealed class CreateMeasurementCommentCommandValidator : AbstractValidator<CreateMeasurementCommentCommand>
{
    public CreateMeasurementCommentCommandValidator()
    {
        // MeasurementId is supplied from the route, not the request body.
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("The comment text is required.")
            .MaximumLength(2000).WithMessage("The comment text must not exceed 2000 characters.");
    }
}
