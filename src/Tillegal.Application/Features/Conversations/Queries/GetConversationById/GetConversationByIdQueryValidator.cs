using FluentValidation;

namespace Tillegal.Application.Features.Conversations.Queries.GetConversationById;

public sealed class GetConversationByIdQueryValidator : AbstractValidator<GetConversationByIdQuery>
{
    public GetConversationByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Conversation ID is required.");
    }
}
