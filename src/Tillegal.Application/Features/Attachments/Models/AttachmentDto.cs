namespace Tillegal.Application.Features.Attachments.Models;

public sealed record AttachmentDto(
    Guid Id,
    Guid ConversationId,
    Guid UserId,
    string FileName,
    string StoragePath,
    long FileSize,
    string ContentType,
    string ProcessingStatus,
    int? TokenCount,
    string? FileHash,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

