using MediatR;

namespace Tillegal.Application.Features.Conversations.Commands.DeleteConversation;

public sealed record DeleteConversationCommand(Guid Id) : IRequest;
