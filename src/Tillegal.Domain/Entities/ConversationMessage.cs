using System.Text.Json;
using Tillegal.Domain.Common;
using Tillegal.Domain.Exceptions;

namespace Tillegal.Domain.Entities;

public sealed class ConversationMessage : BaseEntity
{
    private ConversationMessage() : base()
    {
    }

    public ConversationMessage(
        Guid id,
        Guid conversationId,
        string role,
        string content,
        JsonDocument? citations,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? deletedAt)
        : this(id, conversationId, null, role, content, citations, createdAt, updatedAt, deletedAt)
    {
    }

    public ConversationMessage(
        Guid id,
        Guid conversationId,
        Guid? parentMessageId,
        string role,
        string content,
        JsonDocument? citations,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? deletedAt)
        : base(id, createdAt, updatedAt, deletedAt)
    {
        ConversationId = conversationId;
        ParentMessageId = parentMessageId;
        Role = role;
        Content = content;
        Citations = citations;
    }

    public static ConversationMessage Create(
        Guid conversationId,
        string role,
        string content,
        Guid? parentMessageId = null,
        JsonDocument? citations = null)
    {
        if (conversationId == Guid.Empty)
        {
            throw new DomainException("ConversationId cannot be empty.");
        }

        if (parentMessageId.HasValue && parentMessageId.Value == Guid.Empty)
        {
            throw new DomainException("ParentMessageId cannot be an empty GUID.");
        }

        if (string.IsNullOrWhiteSpace(role))
        {
            throw new DomainException("Role cannot be empty.");
        }

        var normalizedRole = role.Trim().ToLowerInvariant();
        if (normalizedRole is not ("user" or "assistant" or "system"))
        {
            throw new DomainException($"Invalid message role: '{role}'. Allowed roles are 'user', 'assistant', 'system'.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("Message content cannot be empty.");
        }

        return new ConversationMessage
        {
            ConversationId = conversationId,
            ParentMessageId = parentMessageId,
            Role = normalizedRole,
            Content = content.Trim(),
            Citations = citations
        };
    }

    public static ConversationMessage Create(
        Guid conversationId,
        string role,
        string content,
        JsonDocument? citations)
    {
        return Create(conversationId, role, content, null, citations);
    }

    public static ConversationMessage CreateUserMessage(
        Guid conversationId,
        string content,
        Guid? parentMessageId = null)
    {
        return Create(conversationId, "user", content, parentMessageId, null);
    }

    public static ConversationMessage CreateAssistantMessage(
        Guid conversationId,
        string content,
        JsonDocument? citations)
    {
        return Create(conversationId, "assistant", content, null, citations);
    }

    public static ConversationMessage CreateAssistantMessage(
        Guid conversationId,
        string content,
        Guid? parentMessageId = null,
        JsonDocument? citations = null)
    {
        return Create(conversationId, "assistant", content, parentMessageId, citations);
    }

    public Guid ConversationId { get; private set; }

    public Guid? ParentMessageId { get; private set; }

    public string Role { get; private set; } = null!;

    public string Content { get; private set; } = null!;

    public JsonDocument? Citations { get; private set; }

    public void UpdateContent(string newContent, JsonDocument? citations = null)
    {
        if (string.IsNullOrWhiteSpace(newContent))
        {
            throw new DomainException("Message content cannot be empty.");
        }

        Content = newContent.Trim();
        Citations = citations;
        MarkUpdated();
    }
}

