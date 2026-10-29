using System.Text.Json;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;
using Tillegal.Domain.Interfaces.Services;

namespace Tillegal.Domain.Services;

public sealed class ConversationMessageService(
    IConversationMessageRepository messageRepository) : IConversationMessageService
{
    public async Task<ConversationMessage> CreateMessageAsync(
        Guid conversationId,
        string role,
        string content,
        JsonDocument? citations = null,
        CancellationToken cancellationToken = default)
    {
        var message = ConversationMessage.Create(conversationId, role, content, citations);
        await messageRepository.AddAsync(message, cancellationToken);
        return message;
    }

    public async Task<ConversationMessage?> GetMessageByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await messageRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationMessage>> GetMessagesByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await messageRepository.GetMessagesByConversationIdAsync(conversationId, cancellationToken);
    }

    public async Task<ConversationMessage> UpdateMessageAsync(
        ConversationMessage message,
        string newContent,
        JsonDocument? citations = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.UpdateContent(newContent, citations);
        await messageRepository.UpdateAsync(message, cancellationToken);
        return message;
    }

    public async Task DeleteMessageAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.SoftDelete();
        await messageRepository.DeleteAsync(message, cancellationToken);
    }
}
