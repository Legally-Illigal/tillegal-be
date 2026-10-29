using MediatR;
using Tillegal.Application.Features.Conversations.Models;

namespace Tillegal.Application.Features.Conversations.Commands.UpdateConversation;

public sealed record UpdateConversationCommand(Guid Id, string Title) : IRequest<ConversationDto>;
