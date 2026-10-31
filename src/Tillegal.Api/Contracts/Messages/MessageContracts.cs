using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tillegal.Api.Contracts.Messages;

public sealed record CreateMessageApiRequest(
    string? Role,
    string Content,
    JsonDocument? Citations = null);

public sealed record UpdateMessageApiRequest(
    string Content,
    JsonDocument? Citations = null);

public sealed record MessageApiResponse(
    Guid Id,
    Guid ConversationId,
    string Role,
    string Content,
    JsonDocument? Citations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record CursorPagedMessagesApiResponse(
    [property: JsonPropertyName("data")] IReadOnlyList<MessageApiResponse> Data,
    [property: JsonPropertyName("has_more")] bool HasMore,
    [property: JsonPropertyName("next_cursor")] Guid? NextCursor);
