namespace Tillegal.Api.Contracts.Conversations;

public sealed record CreateConversationApiRequest(string? Title);

public sealed record UpdateConversationApiRequest(string Title);

public sealed record ConversationApiResponse(
    Guid Id,
    Guid UserId,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
