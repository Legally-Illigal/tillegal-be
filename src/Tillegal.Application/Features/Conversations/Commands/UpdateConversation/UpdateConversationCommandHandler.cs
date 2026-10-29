using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Conversations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Conversations.Commands.UpdateConversation;

public sealed class UpdateConversationCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<UpdateConversationCommand, ConversationDto>
{
    public async Task<ConversationDto> Handle(
        UpdateConversationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var conversation = await conversationService.GetConversationByIdAsync(
            request.Id,
            cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("Conversation", request.Id);
        }

        if (conversation.UserId != userId)
        {
            throw new ForbiddenAccessException("You do not have access to this conversation.");
        }

        var updatedConversation = await conversationService.UpdateConversationTitleAsync(
            conversation,
            request.Title,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<ConversationDto>(updatedConversation);
    }
}
