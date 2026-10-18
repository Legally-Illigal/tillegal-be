namespace Tillegal.Domain.Common;

public abstract class BaseEntity
{
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }

    protected BaseEntity(
        Guid id,
        DateTimeOffset createdAt,
        DateTimeOffset? updatedAt,
        DateTimeOffset? deletedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    protected void MarkUpdated()
    {
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SoftDelete()
    {
        if (DeletedAt is not null)
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        DeletedAt = now;
        UpdatedAt = now;
    }
}
