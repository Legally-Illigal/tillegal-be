using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Conversations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Conversations.Queries.GetConversationById;

public sealed class GetConversationByIdQueryHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IMapper mapper) : IRequestHandler<GetConversationByIdQuery, ConversationDto>
{
    public async Task<ConversationDto> Handle(
        GetConversationByIdQuery request,
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

        return mapper.Map<ConversationDto>(conversation);
    }
}
