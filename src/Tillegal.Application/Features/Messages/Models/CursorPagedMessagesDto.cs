namespace Tillegal.Application.Features.Messages.Models;

public sealed record CursorPagedMessagesDto(
    IReadOnlyList<MessageDto> Data,
    bool HasMore,
    Guid? NextCursor);

