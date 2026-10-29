using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Messages.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Messages.Commands.CreateMessage;

public sealed class CreateMessageCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IConversationMessageService messageService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(
        CreateMessageCommand request,
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

        var message = await messageService.CreateMessageAsync(
            request.ConversationId,
            request.Role,
            request.Content,
            request.Citations,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<MessageDto>(message);
    }
}
