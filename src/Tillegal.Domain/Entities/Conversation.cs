using Tillegal.Domain.Common;
using Tillegal.Domain.Exceptions;

namespace Tillegal.Domain.Entities;

public sealed class Conversation : BaseEntity
{
    private Conversation() : base()
    {
    }

    public Conversation(
        Guid id,
        Guid userId,
        string title,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? deletedAt)
        : base(id, createdAt, updatedAt, deletedAt)
    {
        UserId = userId;
        Title = title;
    }

    public static Conversation Create(Guid userId, string title)
    {
        if (userId == Guid.Empty)
        {
            throw new DomainException("UserId cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Title cannot be empty.");
        }

        return new Conversation
        {
            UserId = userId,
            Title = title.Trim()
        };
    }

    public Guid UserId { get; private set; }

    public string Title { get; private set; } = null!;

    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new DomainException("Title cannot be empty.");
        }

        Title = newTitle.Trim();
        MarkUpdated();
    }
}
