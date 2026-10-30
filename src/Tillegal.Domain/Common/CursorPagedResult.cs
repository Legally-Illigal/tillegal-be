namespace Tillegal.Domain.Common;

public sealed record CursorPagedResult<T>(
    IReadOnlyList<T> Items,
    bool HasMore,
    Guid? NextCursor);

