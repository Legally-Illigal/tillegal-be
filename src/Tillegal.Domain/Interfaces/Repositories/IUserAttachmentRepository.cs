using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Repositories;

public interface IUserAttachmentRepository
{
    Task<UserAttachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserAttachment>> GetByConversationIdAsync(Guid conversationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserAttachment>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(UserAttachment attachment, CancellationToken cancellationToken = default);

    Task UpdateAsync(UserAttachment attachment, CancellationToken cancellationToken = default);

    Task DeleteAsync(UserAttachment attachment, CancellationToken cancellationToken = default);

    Task HardDeleteAsync(UserAttachment attachment, CancellationToken cancellationToken = default);
}

