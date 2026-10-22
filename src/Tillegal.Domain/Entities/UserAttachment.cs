using Tillegal.Domain.Common;

namespace Tillegal.Domain.Entities;

public sealed class UserAttachment : BaseEntity
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }

    public required string FileName { get; set; }

    public required string StoragePath { get; set; }

    public long FileSize { get; set; }

    public required string ContentType { get; set; }

    public string? ExtractedText { get; set; }

    public string ProcessingStatus { get; set; } = "Pending";

    public int? TokenCount { get; set; }
}
