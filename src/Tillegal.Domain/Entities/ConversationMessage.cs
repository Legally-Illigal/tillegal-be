using System.Text.Json;
using Tillegal.Domain.Common;
using Tillegal.Domain.Exceptions;

namespace Tillegal.Domain.Entities;

public sealed class ConversationMessage : BaseEntity
{
    public ConversationMessage() : base()
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

    public static ConversationMessage CreateUserMessage(Guid conversationId, string content)
    {
        if (conversationId == Guid.Empty)
        {
            throw new DomainException("ConversationId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("Message content cannot be empty.");
        }

        return new ConversationMessage
        {
            ConversationId = conversationId,
            Role = "user",
            Content = content.Trim(),
            Citations = null
        };
    }

    public static ConversationMessage CreateAssistantMessage(
        Guid conversationId,
        string content,
        JsonDocument? citations = null)
    {
        if (conversationId == Guid.Empty)
        {
            throw new DomainException("ConversationId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            throw new DomainException("Message content cannot be empty.");
        }

        return new ConversationMessage
        {
            ConversationId = conversationId,
            Role = "assistant",
            Content = content.Trim(),
            Citations = citations
        };
    }

    public Guid ConversationId { get; set; }

    public string Role { get; set; } = null!;

    public string Content { get; set; } = null!;

    public JsonDocument? Citations { get; set; }
}
