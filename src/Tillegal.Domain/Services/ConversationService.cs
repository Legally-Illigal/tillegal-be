using System.Text.Json;
using Tillegal.Domain.Entities;
using Tillegal.Domain.Interfaces.Repositories;

namespace Tillegal.Domain.Services;

public sealed class ConversationService(
    IConversationRepository conversationRepository,
    IConversationMessageRepository messageRepository)
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

    public async Task<ConversationMessage> AddUserMessageAsync(
        Guid conversationId,
        string content,
        CancellationToken cancellationToken = default)
    {
        var message = ConversationMessage.CreateUserMessage(conversationId, content);
        await messageRepository.AddAsync(message, cancellationToken);
        return message;
    }

    public async Task<ConversationMessage> AddAssistantMessageAsync(
        Guid conversationId,
        string content,
        JsonDocument? citations = null,
        CancellationToken cancellationToken = default)
    {
        var message = ConversationMessage.CreateAssistantMessage(conversationId, content, citations);
        await messageRepository.AddAsync(message, cancellationToken);
        return message;
    }

    public async Task<IReadOnlyList<ConversationMessage>> GetMessagesAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await messageRepository.GetMessagesByConversationIdAsync(conversationId, cancellationToken);
    }
}
