using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.AiEvaluations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.AiEvaluations.Queries.GetEvaluationByMessageId;

public sealed class GetEvaluationByMessageIdQueryHandler(
    ICurrentUserService currentUserService,
    IAiEvaluationService aiEvaluationService,
    IConversationMessageService messageService,
    IConversationService conversationService,
    IMapper mapper) : IRequestHandler<GetEvaluationByMessageIdQuery, AiEvaluationDto>
{
    public async Task<AiEvaluationDto> Handle(
        GetEvaluationByMessageIdQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var evaluation = await aiEvaluationService.GetByMessageIdAsync(request.MessageId, cancellationToken);
        if (evaluation is null)
        {
            throw new NotFoundException("AiEvaluationLog", request.MessageId);
        }

        var isAdmin = string.Equals(currentUserService.Role, "Admin", StringComparison.OrdinalIgnoreCase);
        if (evaluation.EvaluatorUserId == userId || isAdmin)
        {
            return mapper.Map<AiEvaluationDto>(evaluation);
        }

        var message = await messageService.GetMessageByIdAsync(request.MessageId, cancellationToken);
        if (message is not null)
        {
            var conversation = await conversationService.GetConversationByIdAsync(message.ConversationId, cancellationToken);
            if (conversation?.UserId == userId)
            {
                return mapper.Map<AiEvaluationDto>(evaluation);
            }
        }

        throw new ForbiddenAccessException("You do not have permission to view this evaluation.");
    }
}
