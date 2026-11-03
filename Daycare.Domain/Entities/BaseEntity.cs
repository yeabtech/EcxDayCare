namespace Daycare.Domain.Entities;

/// <summary>
/// Abstract base class for all domain entities.
/// Provides common identity and audit field management.
/// </summary>
/// <typeparam name="TId">The strongly-typed ID type (e.g., ChildId)</typeparam>
public abstract class BaseEntity<TId> where TId : TypedId<TId>
{
    /// <summary>
    /// The unique identifier for this entity.
    /// </summary>
    public TId Id { get; protected set; }

    /// <summary>
    /// UTC timestamp when this entity was created.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// UTC timestamp when this entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; protected set; }

    /// <summary>
    /// Protected constructor for derived classes.
    /// Initialize with a new ID and current UTC time.
    /// </summary>
    protected BaseEntity(TId id)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Parameterless constructor for ORM deserialization.
    /// Only used by EF Core and similar frameworks.
    /// </summary>
    protected BaseEntity()
    {
        // ORM will hydrate properties
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp to current UTC time.
    /// Call this whenever the entity is modified.
    /// </summary>
    protected virtual void UpdateTimestamp()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Equality comparison based on ID only.
    /// Two entities with the same ID are considered equal.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;

        if (obj.GetType() != GetType())
            return false;

        var other = (BaseEntity<TId>)obj;
        return Id.Equals(other.Id);
    }

    /// <summary>
    /// Returns the hash code based on ID.
    /// </summary>
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    /// <summary>
    /// Equality operator.
    /// </summary>
    public static bool operator ==(BaseEntity<TId>? left, BaseEntity<TId>? right)
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
    public static bool operator !=(BaseEntity<TId>? left, BaseEntity<TId>? right)
    {
        return !(left == right);
    }
}
