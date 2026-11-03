namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for Child entities.
/// Provides compile-time safety and prevents ID type mixing.
/// </summary>
public sealed class ChildId : TypedId<ChildId>
{
    /// <summary>
    /// Creates a new ChildId with a generated Guid.
    /// </summary>
    public ChildId() { }

    /// <summary>
    /// Creates a ChildId from an existing Guid.
    /// </summary>
    public ChildId(Guid value) : base(value) { }

    /// <summary>
    /// Creates a new ChildId with a randomly generated Guid.
    /// </summary>
    public static ChildId New() => new();

    public static ChildId Create() => new();
    public static ChildId Create(Guid value) => new(value);
    public static ChildId Create(string value) => new(Guid.Parse(value));
}
