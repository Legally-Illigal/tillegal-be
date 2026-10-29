using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Repositories;

public interface IConversationMessageRepository
{
    Task<ConversationMessage?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversationMessage>> GetMessagesByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<ConversationMessage> messages,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default);
}
