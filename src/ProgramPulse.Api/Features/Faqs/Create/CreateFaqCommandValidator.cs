using FluentValidation;

namespace ProgramPulse.Api.Features.Faqs.Create;

public sealed class CreateFaqCommandValidator : AbstractValidator<CreateFaqCommand>
{
    public CreateFaqCommandValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("A question is required.")
            .MaximumLength(1000).WithMessage("The question must not exceed 1000 characters.");

        RuleFor(x => x.Answer)
            .NotEmpty().WithMessage("An answer is required.")
            .MaximumLength(1000).WithMessage("The answer must not exceed 1000 characters.");
    }
}
