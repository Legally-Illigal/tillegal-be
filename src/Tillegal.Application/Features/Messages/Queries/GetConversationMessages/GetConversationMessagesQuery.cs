using MediatR;
using Tillegal.Application.Features.Messages.Models;

namespace Tillegal.Application.Features.Messages.Queries.GetConversationMessages;

public sealed record GetConversationMessagesQuery(
    Guid ConversationId,
    Guid? Cursor = null,
    int Limit = 5) : IRequest<CursorPagedMessagesDto>;
