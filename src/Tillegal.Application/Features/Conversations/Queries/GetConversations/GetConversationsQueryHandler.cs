using MapsterMapper;
using MediatR;
using Tillegal.Application.Common.Exceptions;
using Tillegal.Application.Common.Interfaces;
using Tillegal.Application.Features.Conversations.Models;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Application.Features.Conversations.Queries.GetConversations;

public sealed class GetConversationsQueryHandler(
    ICurrentUserService currentUserService,
    IConversationService conversationService,
    IMapper mapper) : IRequestHandler<GetConversationsQuery, IReadOnlyList<ConversationDto>>
{
    public async Task<IReadOnlyList<ConversationDto>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId
            ?? throw new UnauthorizedException("User is not authenticated.");

        var conversations = await conversationService.GetUserConversationsAsync(
            userId,
            cancellationToken);

        return mapper.Map<IReadOnlyList<ConversationDto>>(conversations);
    }
}
