using MediatR;

namespace Tillegal.Application.Features.Messages.Commands.DeleteMessage;

public sealed record DeleteMessageCommand(
    Guid ConversationId,
    Guid MessageId) : IRequest;
