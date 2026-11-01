using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Commands.UpdateEvaluation;

public sealed class UpdateEvaluationCommandValidator : AbstractValidator<UpdateEvaluationCommand>
{
    public UpdateEvaluationCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Evaluation Id is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.FeedbackComment)
            .MaximumLength(2000)
            .When(x => !string.IsNullOrEmpty(x.FeedbackComment))
            .WithMessage("Feedback comment cannot exceed 2000 characters.");

        RuleFor(x => x)
            .Must(x => x.IsHelpful.HasValue || x.Rating.HasValue || !string.IsNullOrWhiteSpace(x.FeedbackComment))
            .WithMessage("At least one evaluation attribute (isHelpful, rating, or feedbackComment) must be provided.");
    }
}

