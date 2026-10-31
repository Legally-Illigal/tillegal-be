using System.Text.Json;
using MediatR;
using Tillegal.Application.Features.Messages.Models;

namespace Tillegal.Application.Features.Messages.Commands.CreateMessage;

public sealed record CreateMessageCommand(
    Guid ConversationId,
    string Role,
    string Content,
    JsonDocument? Citations = null) : IRequest<MessageDto>;
