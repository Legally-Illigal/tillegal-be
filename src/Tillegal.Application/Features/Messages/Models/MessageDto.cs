using System.Text.Json;

namespace Tillegal.Application.Features.Messages.Models;

public sealed record MessageDto(
    Guid Id,
    Guid ConversationId,
    string Role,
    string Content,
    JsonDocument? Citations,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
