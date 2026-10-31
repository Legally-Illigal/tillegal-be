using System.Text.Json;
using MediatR;
using Tillegal.Application.Features.Messages.Models;

namespace Tillegal.Application.Features.Messages.Commands.UpdateMessage;

public sealed record UpdateMessageCommand(
    Guid ConversationId,
    Guid MessageId,
    string Content,
    JsonDocument? Citations = null) : IRequest<MessageDto>;
