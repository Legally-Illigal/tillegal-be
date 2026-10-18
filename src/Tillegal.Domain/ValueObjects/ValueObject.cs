using System.Collections;

namespace Tillegal.Domain.ValueObjects;

public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object?> GetEqualityComponents();

    public bool Equals(ValueObject? other)
    {
        return other is not null &&
            other.GetType() == GetType() &&
            ComponentsEqual(GetEqualityComponents(), other.GetEqualityComponents());
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(GetType());
        hash.Add(GetComponentsHashCode(GetEqualityComponents()));
        return hash.ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }

    private static bool ComponentsEqual(IEnumerable<object?> left, IEnumerable<object?> right)
    {
        using var leftEnumerator = left.GetEnumerator();
        using var rightEnumerator = right.GetEnumerator();

        while (leftEnumerator.MoveNext())
        {
            if (!rightEnumerator.MoveNext() || !ComponentEqual(leftEnumerator.Current, rightEnumerator.Current))
            {
                return false;
            }
        }

        return !rightEnumerator.MoveNext();
    }

    private static bool ComponentEqual(object? left, object? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.GetType() != right.GetType())
        {
            return false;
        }

        if (left is IEnumerable leftItems && left is not string &&
            right is IEnumerable rightItems && right is not string)
        {
            var leftEnumerator = leftItems.GetEnumerator();
            var rightEnumerator = rightItems.GetEnumerator();
            try
            {
                while (leftEnumerator.MoveNext())
                {
                    if (!rightEnumerator.MoveNext() || !ComponentEqual(leftEnumerator.Current, rightEnumerator.Current))
                    {
                        return false;
                    }
                }

                return !rightEnumerator.MoveNext();
            }
            finally
            {
                (leftEnumerator as IDisposable)?.Dispose();
                (rightEnumerator as IDisposable)?.Dispose();
            }
        }

        return left.Equals(right);
    }

    private static int GetComponentsHashCode(IEnumerable<object?> components)
    {
        var hash = new HashCode();
        foreach (var component in components)
        {
            hash.Add(GetComponentHashCode(component));
        }

        return hash.ToHashCode();
    }

    private static int GetComponentHashCode(object? component)
    {
        if (component is null)
        {
            return 0;
        }

        var hash = new HashCode();
        hash.Add(component.GetType());

        if (component is IEnumerable items && component is not string)
        {
            foreach (var item in items)
            {
                hash.Add(GetComponentHashCode(item));
            }
        }
        else
        {
            hash.Add(component);
        }

        return hash.ToHashCode();
    }
}
