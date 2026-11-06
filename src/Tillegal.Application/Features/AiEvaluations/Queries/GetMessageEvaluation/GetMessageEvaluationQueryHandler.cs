using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetMessageEvaluation;

public sealed class GetMessageEvaluationQueryHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IConversationMessageService messageService,
    IAiEvaluationService aiEvaluationService,
    IMapper mapper) : IRequestHandler<GetMessageEvaluationQuery, AiEvaluationDto>
{
    public async Task<AiEvaluationDto> Handle(
        GetMessageEvaluationQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdmin)
        {
            throw new ForbiddenAccessException("Admin is not allowed to access message evaluations.");
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

        var message = await messageService.GetMessageByIdAsync(
            request.MessageId,
            cancellationToken);

        if (message is null || message.ConversationId != request.ConversationId)
        {
            throw new NotFoundException("Message", request.MessageId);
        }

        if (!string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Only AI assistant messages can have evaluations.");
        }

        var evaluation = await aiEvaluationService.GetEvaluationByMessageIdAsync(
            request.MessageId,
            cancellationToken);

        if (evaluation is null)
        {
            throw new NotFoundException("Evaluation for message", request.MessageId);
        }

        if (evaluation.EvaluatorUserId != userId)
        {
            throw new ForbiddenAccessException("You can only view your own evaluation.");
        }

        return mapper.Map<AiEvaluationDto>(evaluation);
    }
}

