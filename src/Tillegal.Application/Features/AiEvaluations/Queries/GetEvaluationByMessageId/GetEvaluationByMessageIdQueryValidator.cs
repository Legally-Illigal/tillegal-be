using FluentValidation;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationByMessageId;

public sealed class GetEvaluationByMessageIdQueryValidator : AbstractValidator<GetEvaluationByMessageIdQuery>
{
    public GetEvaluationByMessageIdQueryValidator()
    {
        RuleFor(x => x.MessageId)
            .NotEmpty()
            .WithMessage("MessageId is required.");
    }
}

