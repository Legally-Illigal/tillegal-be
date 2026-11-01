using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Exceptions;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Commands.CreateEvaluation;

public sealed class CreateEvaluationCommandHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IConversationMessageService messageService,
    IAiEvaluationService aiEvaluationService,
    IUnitOfWork unitOfWork,
    IMapper mapper) : IRequestHandler<CreateEvaluationCommand, AiEvaluationDto>
{
    public async Task<AiEvaluationDto> Handle(
        CreateEvaluationCommand request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var message = await messageService.GetMessageByIdAsync(request.MessageId, cancellationToken);
        if (message is null)
        {
            throw new NotFoundException("ConversationMessage", request.MessageId);
        }

        if (!string.Equals(message.Role, "assistant", StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("Only AI assistant messages can be evaluated.");
        }

        var conversation = await conversationService.GetConversationByIdAsync(
            message.ConversationId,
            cancellationToken);

        if (conversation is null)
        {
            throw new NotFoundException("Conversation", message.ConversationId);
        }

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (conversation.UserId != userId && !isAdmin)
        {
            throw new ForbiddenAccessException("You do not have permission to evaluate messages in this conversation.");
        }

        var existingEvaluation = await aiEvaluationService.GetByMessageIdAsync(
            request.MessageId,
            cancellationToken);

        if (existingEvaluation is not null)
        {
            throw new DomainException("An evaluation for this message already exists.");
        }

        var evaluation = await aiEvaluationService.CreateEvaluationAsync(
            request.MessageId,
            userId,
            request.IsHelpful,
            request.Rating,
            request.FeedbackComment,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return mapper.Map<AiEvaluationDto>(evaluation);
    }
}
