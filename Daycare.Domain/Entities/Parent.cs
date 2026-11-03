using Daycare.Domain.Results;
using Daycare.Domain.ValueObjects;

namespace Daycare.Domain.Entities;

/// <summary>
/// Parent/Guardian entity - represents a parent or guardian of children in the daycare
/// </summary>
public sealed class Parent : BaseEntity<ParentId>
{
    public ParentId ParentId => Id;

    public PersonName Name { get; private set; } = null!;

    public ContactInfo ContactInfo { get; private set; } = null!;

    public string Address { get; private set; } = string.Empty;

    public ParentStatus Status { get; private set; } = ParentStatus.Active;

    public string? Notes { get; private set; }

    // Navigation property
    public ICollection<ParentChild> ParentChildren { get; private set; } = new List<ParentChild>();

    private Parent() { }

    public Parent(
        ParentId id,
        PersonName name,
        ContactInfo contactInfo,
        string address,
        ParentStatus status = ParentStatus.Active,
        string? notes = null)
        : base(id)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Status = status;
        Notes = notes;
    }

    /// <summary>
    /// Create a new parent
    /// </summary>
    public static Result<Parent> Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string address,
        ParentStatus status = ParentStatus.Active,
        string? notes = null)
    {
        // Validate input
        var nameResult = PersonName.Create(firstName, lastName);
        if (nameResult.IsFailure)
            return Result<Parent>.Failure(nameResult.Error);

        var contactResult = ContactInfo.Create(email, phoneNumber);
        if (contactResult.IsFailure)
            return Result<Parent>.Failure(contactResult.Error);

        if (string.IsNullOrWhiteSpace(address))
            return Result<Parent>.Failure("Address is required");

        var parent = new Parent(
            ParentId.Create(),
            nameResult.Value,
            contactResult.Value,
            address.Trim(),
            status,
            notes?.Trim());

        return Result<Parent>.Success(parent);
    }

    /// <summary>
    /// Update parent information
    /// </summary>
    public Result Update(
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        string address,
        ParentStatus status,
        string? notes = null)
    {
        var nameResult = PersonName.Create(firstName, lastName);
        if (nameResult.IsFailure)
            return Result.Failure(nameResult.Error);

        var contactResult = ContactInfo.Create(email, phoneNumber);
        if (contactResult.IsFailure)
            return Result.Failure(contactResult.Error);

        if (string.IsNullOrWhiteSpace(address))
            return Result.Failure("Address is required");

        Name = nameResult.Value;
        ContactInfo = contactResult.Value;
        Address = address.Trim();
        Status = status;
        Notes = notes?.Trim();
        UpdateTimestamp();

        return Result.Success();
    }

    /// <summary>
    /// Link a child to this parent
    /// </summary>
    public Result AddChild(ChildId childId, ParentRole role = ParentRole.Guardian)
    {
        if (childId is null)
            return Result.Failure("Child ID is required");

        if (ParentChildren.Any(pc => pc.ChildId == childId))
            return Result.Failure("Child is already linked to this parent");

        var parentChild = new ParentChild
        {
            ParentId = ParentId,
            ChildId = childId,
            Role = role
        };

        ParentChildren.Add(parentChild);
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Remove a child from this parent
    /// </summary>
    public Result RemoveChild(ChildId childId)
    {
        var parentChild = ParentChildren.FirstOrDefault(pc => pc.ChildId == childId);
        if (parentChild is null)
            return Result.Failure("Child is not linked to this parent");

        ParentChildren.Remove(parentChild);
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Deactivate this parent
    /// </summary>
    public void Deactivate()
    {
        Status = ParentStatus.Inactive;
        UpdateTimestamp();
    }

    /// <summary>
    /// Activate this parent
    /// </summary>
    public void Activate()
    {
        Status = ParentStatus.Active;
        UpdateTimestamp();
    }
}

/// <summary>
/// Strongly-typed Parent ID
/// </summary>
public sealed class ParentId : TypedId<ParentId>
{
    public ParentId() { }
    public ParentId(Guid value) : base(value) { }

    public static ParentId New() => new();
    public static ParentId Create() => new();
    public static ParentId Create(Guid value) => new(value);
    public static ParentId Create(string value) => new(Guid.Parse(value));
}

/// <summary>
/// Join table for Parent-Child many-to-many relationship
/// </summary>
public sealed class ParentChild
{
    public ParentId ParentId { get; set; } = null!;
    public ChildId ChildId { get; set; } = null!;

    /// <summary>
    /// The role this parent has for the child (Mother, Father or Guardian).
    /// </summary>
    public ParentRole Role { get; set; } = ParentRole.Guardian;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Parent Parent { get; set; } = null!;
    public Child Child { get; set; } = null!;
}

/// <summary>
/// Lightweight description of a parent-child link used by the application layer
/// when creating or replacing the parents of a child.
/// </summary>
public sealed record ParentChildLink(ParentId ParentId, ParentRole Role);
