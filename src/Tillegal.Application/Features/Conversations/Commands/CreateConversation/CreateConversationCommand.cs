using MediatR;
using Tillegal.Application.Features.Conversations.Models;

namespace Tillegal.Application.Features.Conversations.Commands.CreateConversation;

public sealed record CreateConversationCommand(string? Title = null) : IRequest<ConversationDto>;
