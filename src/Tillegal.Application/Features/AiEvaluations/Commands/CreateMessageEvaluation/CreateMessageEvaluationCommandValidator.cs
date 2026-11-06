using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Commands.CreateMessageEvaluation;

public sealed class CreateMessageEvaluationCommandValidator : AbstractValidator<CreateMessageEvaluationCommand>
{
    public CreateMessageEvaluationCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.MessageId)
            .NotEmpty().WithMessage("MessageId is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .When(x => x.Rating.HasValue)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x)
            .Must(x => x.IsHelpful.HasValue || x.Rating.HasValue || !string.IsNullOrWhiteSpace(x.FeedbackComment))
            .WithMessage("At least one evaluation attribute (isHelpful, rating, or feedbackComment) must be provided.");
    }
}

