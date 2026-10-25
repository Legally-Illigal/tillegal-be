using MediatR;
using Tillegal.Application.Features.Conversations.Commands.AskQuestion;

namespace Tillegal.Application.Features.Conversations.Queries.GetConversationMessages;

public sealed record GetConversationMessagesQuery(Guid ConversationId) : IRequest<IReadOnlyList<ConversationMessageDto>>;
