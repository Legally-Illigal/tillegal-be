namespace Tillegal.Api.Contracts.Attachments;

public sealed record AttachmentApiResponse(
    Guid Id,
    Guid ConversationId,
    Guid UserId,
    string FileName,
    long FileSize,
    string ContentType,
    string ProcessingStatus,
    int? TokenCount,
    string? FileHash,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

