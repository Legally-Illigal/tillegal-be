using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Services;

public interface IConversationService
{
    Task<Conversation> CreateConversationAsync(
        Guid userId,
        string title,
        CancellationToken cancellationToken = default);

    Task<Conversation?> GetConversationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Conversation> UpdateConversationTitleAsync(
        Conversation conversation,
        string newTitle,
        CancellationToken cancellationToken = default);

    Task DeleteConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default);
}
