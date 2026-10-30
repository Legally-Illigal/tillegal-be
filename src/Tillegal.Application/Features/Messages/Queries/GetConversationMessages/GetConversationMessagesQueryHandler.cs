using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Messages.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Messages.Queries.GetConversationMessages;

public sealed class GetConversationMessagesQueryHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IConversationMessageService messageService,
    IMapper mapper) : IRequestHandler<GetConversationMessagesQuery, CursorPagedMessagesDto>
{
    public async Task<CursorPagedMessagesDto> Handle(
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
            throw new NotFoundException("Conversation", request.ConversationId);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenAccessException("You do not have access to this conversation.");
        }

        var pagedResult = await messageService.GetMessagesByConversationIdAsync(
            request.ConversationId,
            request.Cursor,
            request.Limit,
            cancellationToken);

        var messageDtos = mapper.Map<IReadOnlyList<MessageDto>>(pagedResult.Items);

        return new CursorPagedMessagesDto(messageDtos, pagedResult.HasMore, pagedResult.NextCursor);
    }
}
