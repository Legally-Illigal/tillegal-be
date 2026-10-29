using MediatR;
using Tillegal.Application.Features.Messages.Models;

namespace Tillegal.Application.Features.Messages.Queries.GetConversationMessages;

public sealed record GetConversationMessagesQuery(Guid ConversationId) : IRequest<IReadOnlyList<MessageDto>>;
