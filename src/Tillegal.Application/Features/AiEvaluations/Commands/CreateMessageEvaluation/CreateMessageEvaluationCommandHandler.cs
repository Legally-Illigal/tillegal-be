using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Commands.CreateMessageEvaluation;

public sealed class CreateMessageEvaluationCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IConversationMessageService messageService,
    IAiEvaluationService aiEvaluationService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateMessageEvaluationCommand, AiEvaluationDto>
{
    public async Task<AiEvaluationDto> Handle(
        CreateMessageEvaluationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (isAdmin)
        {
            throw new ForbiddenAccessException("Admin is not allowed to evaluate.");
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
            throw new DomainException("Only AI assistant messages can be evaluated.");
        }

        var exists = await aiEvaluationService.ExistsForMessageAsync(request.MessageId, cancellationToken);
        if (exists)
        {
            throw new DomainException("An evaluation already exists for this message. Use PUT to update it.");
        }

        var evaluation = await aiEvaluationService.CreateEvaluationAsync(
            request.MessageId,
            userId,
            request.IsHelpful,
            request.Rating,
            request.FeedbackComment,
            currentUserService.Role,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<AiEvaluationDto>(evaluation);
    }
}

