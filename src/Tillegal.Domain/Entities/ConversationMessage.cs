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
        : base(id, createdAt, updatedAt, deletedAt)
    {
        ConversationId = conversationId;
        Role = role;
        Content = content;
        Citations = citations;
    }

    public static ConversationMessage Create(
        Guid conversationId,
        string role,
        string content,
        JsonDocument? citations = null)
    {
        if (conversationId == Guid.Empty)
        {
            throw new DomainException("ConversationId cannot be empty.");
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
            Role = normalizedRole,
            Content = content.Trim(),
            Citations = citations
        };
    }

    public static ConversationMessage CreateUserMessage(Guid conversationId, string content)
    {
        return Create(conversationId, "user", content, null);
    }

    public static ConversationMessage CreateAssistantMessage(
        Guid conversationId,
        string content,
        JsonDocument? citations = null)
    {
        return Create(conversationId, "assistant", content, citations);
    }

    public Guid ConversationId { get; private set; }

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

