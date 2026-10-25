using System.Text.Json;

namespace Tillegal.Api.Contracts.Conversations;

public sealed record CreateConversationApiRequest(string? Title = null);

public sealed record CreateConversationApiResponse(
    Guid Id,
    Guid UserId,
    string Title,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record AskQuestionApiRequest(string Content);

public sealed record ConversationMessageApiResponse(
    Guid Id,
    Guid ConversationId,
    string Role,
    string Content,
    JsonDocument? Citations,
    DateTimeOffset CreatedAt);

public sealed record AskQuestionApiResponse(
    ConversationMessageApiResponse UserMessage,
    ConversationMessageApiResponse AssistantMessage);
