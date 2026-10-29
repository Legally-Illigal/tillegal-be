using System.Text.Json;
using Tillegal.Domain.Entities;

namespace Tillegal.Domain.Interfaces.Services;

public interface IConversationMessageService
{
    Task<ConversationMessage> CreateMessageAsync(
        Guid conversationId,
        string role,
        string content,
        JsonDocument? citations = null,
        CancellationToken cancellationToken = default);

    Task<ConversationMessage?> GetMessageByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConversationMessage>> GetMessagesByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<ConversationMessage> UpdateMessageAsync(
        ConversationMessage message,
        string newContent,
        JsonDocument? citations = null,
        CancellationToken cancellationToken = default);

    Task DeleteMessageAsync(
        ConversationMessage message,
        CancellationToken cancellationToken = default);
}
