using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Commands.DeleteMessageEvaluation;

public sealed class DeleteMessageEvaluationCommandValidator : AbstractValidator<DeleteMessageEvaluationCommand>
{
    public DeleteMessageEvaluationCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.MessageId)
            .NotEmpty().WithMessage("MessageId is required.");
    }
}

