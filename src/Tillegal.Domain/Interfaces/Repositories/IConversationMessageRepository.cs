using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Repositories;

public interface IConversationMessageRepository
{
    Task<IReadOnlyList<ConversationMessage>> GetMessagesByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default);

    Task AddRangeAsync(
        IEnumerable<ConversationMessage> messages,
        CancellationToken cancellationToken = default);
}
