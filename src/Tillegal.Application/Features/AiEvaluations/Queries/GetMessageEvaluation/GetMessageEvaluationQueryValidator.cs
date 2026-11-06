using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetMessageEvaluation;

public sealed class GetMessageEvaluationQueryValidator : AbstractValidator<GetMessageEvaluationQuery>
{
    public GetMessageEvaluationQueryValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.MessageId)
            .NotEmpty().WithMessage("MessageId is required.");
    }
}

