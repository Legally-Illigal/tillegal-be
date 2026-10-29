using System.Text.Json;

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
