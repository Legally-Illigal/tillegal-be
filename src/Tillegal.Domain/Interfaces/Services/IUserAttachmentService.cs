using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Services;

public interface IUserAttachmentService
{
    Task<UserAttachment> CreateAttachmentAsync(
        Guid conversationId,
        Guid userId,
        string fileName,
        string storagePath,
        long fileSize,
        string contentType,
        string? fileHash = null,
        CancellationToken cancellationToken = default);

    Task<UserAttachment?> GetAttachmentByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserAttachment>> GetAttachmentsByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserAttachment>> GetAttachmentsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task DeleteAttachmentAsync(UserAttachment attachment, CancellationToken cancellationToken = default);
}

