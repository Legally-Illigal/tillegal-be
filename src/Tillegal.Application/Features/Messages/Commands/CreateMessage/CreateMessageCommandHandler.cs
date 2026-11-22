using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Common.Models;
using Tillegal.Application.Features.Messages.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Messages.Commands.CreateMessage;

public sealed class CreateMessageCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IConversationMessageService messageService,
    IAiServiceClient aiServiceClient,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateMessageCommand, MessageDto>
{
    public async Task<MessageDto> Handle(
        CreateMessageCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdmin)
        {
            throw new ForbiddenAccessException("Admin is not allowed to send messages.");
        }

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

        if (request.ParentMessageId.HasValue && request.ParentMessageId.Value != Guid.Empty)
        {
            var parentMessage = await messageService.GetMessageByIdAsync(
                request.ParentMessageId.Value,
                cancellationToken);

            if (parentMessage is null || parentMessage.ConversationId != request.ConversationId)
            {
                throw new NotFoundException("Message", request.ParentMessageId.Value);
            }
        }

        var isUserMessage = string.Equals(request.Role, "user", StringComparison.OrdinalIgnoreCase);
        if (!isUserMessage)
        {
            var singleMessage = await messageService.CreateMessageAsync(
                request.ConversationId,
                request.Role,
                request.Content,
                request.ParentMessageId,
                request.Citations,
                cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return mapper.Map<MessageDto>(singleMessage);
        }

        var userMessage = await messageService.CreateMessageAsync(
            request.ConversationId,
            request.Role,
            request.Content,
            request.ParentMessageId,
            request.Citations,
            cancellationToken);

        var isLawyer = string.Equals(currentUserService.Role, "Lawyer", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(currentUserService.Role, "Lawer", StringComparison.OrdinalIgnoreCase);
        var workflowMode = isLawyer ? "multi_agent" : "single_agent";

        var previousMessages = await messageService.GetMessagesByConversationIdAsync(
            request.ConversationId,
            cancellationToken);

        var history = previousMessages
            .TakeLast(10)
            .Select(m => new ConversationHistoryItem(m.Role, m.Content))
            .ToList();

        var aiResult = await aiServiceClient.ConsultLegalAsync(
            request.Content,
            workflowMode,
            history,
            cancellationToken);

        var assistantMessage = await messageService.CreateMessageAsync(
            request.ConversationId,
            "assistant",
            aiResult.Content,
            userMessage.Id,
            aiResult.Citations,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<MessageDto>(assistantMessage);
    }
}
