using MediatR;
using Tillegal.Application.Features.Messages.Models;

namespace Tillegal.Application.Features.Messages.Queries.GetMessageById;

public sealed record GetMessageByIdQuery(
    Guid ConversationId,
    Guid MessageId) : IRequest<MessageDto>;
