using MediatR;

namespace Tillegal.Application.Features.Conversations.Commands.CreateConversation;

public sealed record CreateConversationCommand(string? Title = null) : IRequest<CreateConversationResult>;

public sealed record CreateConversationResult(
    Guid Id,
    Guid UserId,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
