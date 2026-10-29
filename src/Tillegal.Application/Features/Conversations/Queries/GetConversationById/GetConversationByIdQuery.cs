using MediatR;
using Tillegal.Application.Features.Conversations.Models;

namespace Tillegal.Application.Features.Conversations.Queries.GetConversationById;

public sealed record GetConversationByIdQuery(Guid Id) : IRequest<ConversationDto>;
