using FluentValidation;

namespace Tillegal.Application.Features.Conversations.Commands.AskQuestion;

public sealed class AskQuestionCommandValidator : AbstractValidator<AskQuestionCommand>
{
    public AskQuestionCommandValidator()
    {
        RuleFor(x => x.ConversationId)
            .NotEmpty().WithMessage("ConversationId is required.");

        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required.")
            .MaximumLength(4000).WithMessage("Question must not exceed 4000 characters.")
            .Must(q => !string.IsNullOrWhiteSpace(q)).WithMessage("Question cannot consist only of whitespace.");
    }
}
