using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Domain.Services;

public sealed class ConversationService(IConversationRepository conversationRepository) : IConversationService
{
    public async Task<Conversation> CreateConversationAsync(
        Guid userId,
        string title,
        CancellationToken cancellationToken = default)
    {
        var conversation = Conversation.Create(userId, title);
        await conversationRepository.AddAsync(conversation, cancellationToken);
        return conversation;
    }

    public async Task<Conversation?> GetConversationByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await conversationRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<Conversation>> GetUserConversationsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await conversationRepository.GetByUserIdAsync(userId, cancellationToken);
    }

    public async Task<Conversation> UpdateConversationTitleAsync(
        Conversation conversation,
        string newTitle,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        conversation.UpdateTitle(newTitle);
        await conversationRepository.UpdateAsync(conversation, cancellationToken);
        return conversation;
    }

    public async Task DeleteConversationAsync(
        Conversation conversation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conversation);

        conversation.SoftDelete();
        await conversationRepository.DeleteAsync(conversation, cancellationToken);
    }
}
