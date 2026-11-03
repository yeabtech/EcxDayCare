namespace Daycare.Domain.Entities;

/// <summary>
/// Abstract base class for strongly-typed identifiers.
/// Prevents accidental mixing of different ID types at compile time.
/// 
/// Usage: Create specific ID types like:
///   public sealed class ChildId : TypedId&lt;ChildId&gt; { }
///   public sealed class StaffId : TypedId&lt;StaffId&gt; { }
/// </summary>
/// <typeparam name="T">The derived TypedId type (e.g., ChildId)</typeparam>
public abstract class TypedId<T> : IEquatable<TypedId<T>> where T : TypedId<T>
{
    /// <summary>
    /// The underlying value of the ID (typically a Guid or int).
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Protected constructor to prevent direct instantiation.
    /// Subclasses must be sealed.
    /// </summary>
    protected TypedId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("ID value cannot be empty.", nameof(value));
        }

        Value = value;
    }

    /// <summary>
    /// Creates a new TypedId with a random Guid.
    /// </summary>
    protected TypedId() : this(Guid.NewGuid())
    {
    }

    /// <summary>
    /// Equality comparison based on the underlying value.
    /// </summary>
    public bool Equals(TypedId<T>? other)
    {
        if (other is null)
            return false;

        return Value == other.Value;
    }

    /// <summary>
    /// Equality check with objects of any type.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj.GetType() != GetType())
            return false;

        return Equals((TypedId<T>)obj);
    }

    /// <summary>
    /// Returns the hash code of the underlying value.
    /// </summary>
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    /// <summary>
    /// Returns the string representation of the ID.
    /// </summary>
    public override string ToString()
    {
        return Value.ToString();
    }

    /// <summary>
    /// Converts a Guid to this TypedId type.
    /// </summary>
    public static T From(Guid value)
    {
        var instance = Activator.CreateInstance(typeof(T), value) as T
            ?? throw new InvalidOperationException($"Failed to create instance of {typeof(T).Name}");

        return instance;
    }

    /// <summary>
    /// Attempts to parse a string representation to this TypedId type.
    /// </summary>
    public static bool TryParse(string? value, out T? result)
    {
        result = null;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (!Guid.TryParse(value, out var guid))
            return false;

        if (guid == Guid.Empty)
            return false;

        result = From(guid);
        return true;
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(TypedId<T>? left, TypedId<T>? right)
    {
        if (left is null && right is null)
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    /// <summary>
    /// Inequality operator.
    /// </summary>
    public static bool operator !=(TypedId<T>? left, TypedId<T>? right)
    {
        return !(left == right);
    }
}
