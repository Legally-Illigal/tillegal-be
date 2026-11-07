using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Domain.Services;

public sealed class UserAttachmentService(IUserAttachmentRepository attachmentRepository) : IUserAttachmentService
{
    public async Task<UserAttachment> CreateAttachmentAsync(
        Guid conversationId,
        Guid userId,
        string fileName,
        string storagePath,
        long fileSize,
        string contentType,
        string? fileHash = null,
        CancellationToken cancellationToken = default)
    {
        var attachment = UserAttachment.Create(
            conversationId,
            userId,
            fileName,
            storagePath,
            fileSize,
            contentType,
            fileHash);

        await attachmentRepository.AddAsync(attachment, cancellationToken);
        return attachment;
    }

    public async Task<UserAttachment?> GetAttachmentByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await attachmentRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<UserAttachment>> GetAttachmentsByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await attachmentRepository.GetByConversationIdAsync(conversationId, cancellationToken);
    }

    public async Task<IReadOnlyList<UserAttachment>> GetAttachmentsByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await attachmentRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task DeleteAttachmentAsync(
        UserAttachment attachment,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(attachment);

        if (string.Equals(attachment.ProcessingStatus, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            await attachmentRepository.HardDeleteAsync(attachment, cancellationToken);
        }
        else
        {
            attachment.SoftDelete();
            await attachmentRepository.DeleteAsync(attachment, cancellationToken);
        }
    }
}

