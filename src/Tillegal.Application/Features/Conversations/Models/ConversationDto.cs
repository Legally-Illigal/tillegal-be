namespace Tillegal.Application.Features.Conversations.Models;

public sealed record ConversationDto(
    Guid Id,
    Guid UserId,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
