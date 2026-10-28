using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Conversations.Commands.DeleteConversation;

public sealed class DeleteConversationCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteConversationCommand>
{
    public async Task Handle(
        DeleteConversationCommand request,
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

        await conversationService.DeleteConversationAsync(conversation, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
