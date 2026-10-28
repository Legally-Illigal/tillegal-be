using MediatR;
using Tillegal.Application.Features.Conversations.Models;

namespace Tillegal.Application.Features.Conversations.Queries.GetConversations;

public sealed record GetConversationsQuery : IRequest<IReadOnlyList<ConversationDto>>;
