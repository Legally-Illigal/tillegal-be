using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Services;

namespace Tillegal.Application.Features.Conversations.Commands.AskQuestion;

public sealed class AskQuestionCommandHandler(
    ICurrentUserService currentUserService,
    ConversationService conversationService,
    ILegalAiService legalAiService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<AskQuestionCommand, AskQuestionResult>
{
    public async Task<AskQuestionResult> Handle(
        AskQuestionCommand request,
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

        // Retrieve existing messages to build historical context
        var existingMessages = await conversationService.GetMessagesAsync(
            request.ConversationId,
            cancellationToken);

        var history = existingMessages
            .Select(m => new AiMessageContext(m.Role, m.Content))
            .ToList();

        // 1. Add user question message
        var userMessage = await conversationService.AddUserMessageAsync(
            request.ConversationId,
            request.Question,
            cancellationToken);

        // 2. Request AI legal analysis and citations
        var aiAnswer = await legalAiService.AskAsync(
            new AiQuestionContext(request.ConversationId, request.Question, history),
            cancellationToken);

        // 3. Add AI assistant message
        var assistantMessage = await conversationService.AddAssistantMessageAsync(
            request.ConversationId,
            aiAnswer.Content,
            aiAnswer.Citations,
            cancellationToken);

        // 4. Commit transaction / persist both messages
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AskQuestionResult(
            mapper.Map<ConversationMessageDto>(userMessage),
            mapper.Map<ConversationMessageDto>(assistantMessage));
    }
}
