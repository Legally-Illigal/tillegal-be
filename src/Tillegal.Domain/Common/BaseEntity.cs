namespace Tillegal.Domain.Common;

public abstract class BaseEntity : IEquatable<BaseEntity>
{
    protected BaseEntity()
        : this(Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow)
    {
    }

    protected BaseEntity(
        Guid id,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        DateTimeOffset? deletedAt = null)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException("Entity ID cannot be empty.", nameof(id));
        }

        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
        DeletedAt = deletedAt;
    }

    public Guid Id { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public bool Equals(BaseEntity? other)
    {
        return other is not null &&
            other.GetType() == GetType() &&
            other.Id == Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is BaseEntity other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(BaseEntity? left, BaseEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseEntity? left, BaseEntity? right)
    {
        return !Equals(left, right);
    }
}
