using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Conversations.Commands.AskQuestion;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Services;

namespace Tillegal.Application.Features.Conversations.Queries.GetConversationMessages;

public sealed class GetConversationMessagesQueryHandler(
    ICurrentUserService currentUserService,
    ConversationService conversationService,
    IMapper mapper) : IRequestHandler<GetConversationMessagesQuery, IReadOnlyList<ConversationMessageDto>>
{
    public async Task<IReadOnlyList<ConversationMessageDto>> Handle(
        GetConversationMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var conversation = await conversationService.GetConversationByIdAsync(
            request.ConversationId,
            cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException(nameof(Conversation), request.ConversationId);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenAccessException("You do not have permission to access this conversation.");
        }

        var messages = await conversationService.GetMessagesAsync(
            request.ConversationId,
            cancellationToken);

        return mapper.Map<IReadOnlyList<ConversationMessageDto>>(messages);
    }
}
