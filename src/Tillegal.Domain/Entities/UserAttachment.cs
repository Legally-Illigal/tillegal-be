using Tillegal.Domain.Common;
using Tillegal.Domain.Exceptions;

namespace Tillegal.Domain.Entities;

public sealed class UserAttachment : BaseEntity
{
    private UserAttachment() : base()
    {
    }

    public UserAttachment(
        Guid id,
        Guid conversationId,
        Guid userId,
        string fileName,
        string storagePath,
        long fileSize,
        string contentType,
        string processingStatus,
        int? tokenCount,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? deletedAt,
        string? fileHash = null)
        : base(id, createdAt, updatedAt, deletedAt)
    {
        ConversationId = conversationId;
        UserId = userId;
        FileName = fileName;
        StoragePath = storagePath;
        FileSize = fileSize;
        ContentType = contentType;
        ProcessingStatus = processingStatus;
        TokenCount = tokenCount;
        FileHash = fileHash;
    }

    public static UserAttachment Create(
        Guid conversationId,
        Guid userId,
        string fileName,
        string storagePath,
        long fileSize,
        string contentType,
        string? fileHash = null)
    {
        if (conversationId == Guid.Empty)
        {
            throw new DomainException("ConversationId cannot be empty.");
        }

        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new DomainException("FileName cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new DomainException("StoragePath cannot be empty.");
        }

        if (fileSize <= 0)
        {
            throw new DomainException("FileSize must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new DomainException("ContentType cannot be empty.");
        }

        return new UserAttachment
        {
            ConversationId = conversationId,
            UserId = userId,
            FileName = fileName.Trim(),
            StoragePath = storagePath.Trim(),
            FileSize = fileSize,
            ContentType = contentType.Trim(),
            ProcessingStatus = "Pending",
            FileHash = string.IsNullOrWhiteSpace(fileHash) ? null : fileHash.Trim().ToLowerInvariant()
        };
    }

    public Guid ConversationId { get; private set; }

    public Guid UserId { get; private set; }

    public string FileName { get; private set; } = null!;

    public string StoragePath { get; private set; } = null!;

    public long FileSize { get; private set; }

    public string ContentType { get; private set; } = null!;

    public string ProcessingStatus { get; private set; } = "Pending";

    public int? TokenCount { get; private set; }

    public string? FileHash { get; private set; }

    public void UpdateProcessingStatus(string status, int? tokenCount = null)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            throw new DomainException("ProcessingStatus cannot be empty.");
        }

        ProcessingStatus = status.Trim();
        TokenCount = tokenCount;
        MarkUpdated();
    }
}
