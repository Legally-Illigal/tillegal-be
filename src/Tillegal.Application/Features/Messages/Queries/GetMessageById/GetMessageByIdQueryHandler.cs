using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Messages.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Messages.Queries.GetMessageById;

public sealed class GetMessageByIdQueryHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IConversationMessageService messageService,
    IMapper mapper) : IRequestHandler<GetMessageByIdQuery, MessageDto>
{
    public async Task<MessageDto> Handle(
        GetMessageByIdQuery request,
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

        var message = await messageService.GetMessageByIdAsync(
            request.MessageId,
            cancellationToken);

        if (message is null || message.ConversationId != request.ConversationId)
        {
            throw new NotFoundException("Message", request.MessageId);
        }

        return mapper.Map<MessageDto>(message);
    }
}
