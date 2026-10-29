using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);

    Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default);

    Task DeleteAsync(Conversation conversation, CancellationToken cancellationToken = default);
}
