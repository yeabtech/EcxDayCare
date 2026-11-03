using Daycare.Domain.Results;
using Daycare.Domain.ValueObjects;

namespace Daycare.Domain.Entities;

/// <summary>
/// Classroom entity - represents a classroom in the daycare with capacity and age group
/// </summary>
public sealed class Classroom : BaseEntity<ClassroomId>
{
    public ClassroomId ClassroomId => Id;

    public string Name { get; private set; } = string.Empty;

    public AgeGroup AgeGroup { get; private set; } = null!;

    public ClassroomCapacity Capacity { get; private set; } = null!;

    public ClassroomStatus Status { get; private set; } = ClassroomStatus.Active;

    public string? Description { get; private set; }

    public Guid? LeadTeacherId { get; private set; }

    // Navigation property
    public ICollection<Child> Children { get; private set; } = new List<Child>();

    public ICollection<Staff> StaffMembers { get; private set; } = new List<Staff>();

    private Classroom() { }

    public Classroom(
        ClassroomId id,
        string name,
        AgeGroup ageGroup,
        ClassroomCapacity capacity,
        ClassroomStatus status = ClassroomStatus.Active,
        Guid? leadTeacherId = null,
        string? description = null)
        : base(id)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        AgeGroup = ageGroup ?? throw new ArgumentNullException(nameof(ageGroup));
        Capacity = capacity ?? throw new ArgumentNullException(nameof(capacity));
        Status = status;
        LeadTeacherId = leadTeacherId;
        Description = description;
    }

    /// <summary>
    /// Create a new classroom
    /// </summary>
    public static Result<Classroom> Create(
        string name,
        int minAgeMonths,
        int maxAgeMonths,
        int maxCapacity,
        Guid? leadTeacherId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Classroom>.Failure("Classroom name is required");

        if (name.Length > 100)
            return Result<Classroom>.Failure("Classroom name must not exceed 100 characters");

        var ageGroupResult = AgeGroup.Create(minAgeMonths, maxAgeMonths);
        if (ageGroupResult.IsFailure)
            return Result<Classroom>.Failure(ageGroupResult.Error);

        var capacityResult = ClassroomCapacity.Create(maxCapacity);
        if (capacityResult.IsFailure)
            return Result<Classroom>.Failure(capacityResult.Error);

        var classroom = new Classroom(
            ClassroomId.Create(),
            name.Trim(),
            ageGroupResult.Value,
            capacityResult.Value,
            ClassroomStatus.Active,
            leadTeacherId,
            description?.Trim());

        return Result<Classroom>.Success(classroom);
    }

    /// <summary>
    /// Update classroom information
    /// </summary>
    public Result Update(
        string name,
        int minAgeMonths,
        int maxAgeMonths,
        int maxCapacity,
        ClassroomStatus status,
        Guid? leadTeacherId = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure("Classroom name is required");

        var ageGroupResult = AgeGroup.Create(minAgeMonths, maxAgeMonths);
        if (ageGroupResult.IsFailure)
            return Result.Failure(ageGroupResult.Error);

        var capacityResult = ClassroomCapacity.Create(maxCapacity);
        if (capacityResult.IsFailure)
            return Result.Failure(capacityResult.Error);

        // Check if current children count exceeds new capacity
        if (Children.Count > maxCapacity)
            return Result.Failure($"Cannot reduce capacity below current enrollment ({Children.Count} children)");

        Name = name.Trim();
        AgeGroup = ageGroupResult.Value;
        Capacity = capacityResult.Value;
        Status = status;
        LeadTeacherId = leadTeacherId;
        Description = description?.Trim();
        UpdateTimestamp();

        return Result.Success();
    }

    /// <summary>
    /// Assigns or updates the lead teacher for the classroom
    /// </summary>
    public Result AssignLeadTeacher(Guid? leadTeacherId)
    {
        LeadTeacherId = leadTeacherId;
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Add a child to classroom
    /// </summary>
    public Result AddChild(Child child)
    {
        if (child is null)
            return Result.Failure("Child is required");

        if (Children.Contains(child))
            return Result.Failure("Child is already in this classroom");

        if (Children.Count >= Capacity.MaxCapacity)
            return Result.Failure($"Classroom is at full capacity ({Capacity.MaxCapacity})");

        Children.Add(child);
        child.AssignToClassroom(Id);
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Remove a child from classroom
    /// </summary>
    public Result RemoveChild(Child child)
    {
        if (child is null)
            return Result.Failure("Child is required");

        if (!Children.Contains(child))
            return Result.Failure("Child is not in this classroom");

        Children.Remove(child);
        child.RemoveFromClassroom();
        UpdateTimestamp();
        return Result.Success();
    }

    /// <summary>
    /// Get current enrollment count
    /// </summary>
    public int GetCurrentEnrollment() => Children.Count;

    /// <summary>
    /// Get available spots
    /// </summary>
    public int GetAvailableSpots() => Capacity.MaxCapacity - Children.Count;

    /// <summary>
    /// Check if classroom is at capacity
    /// </summary>
    public bool IsAtCapacity() => Children.Count >= Capacity.MaxCapacity;
}

/// <summary>
/// Strongly-typed Classroom ID
/// </summary>
public sealed class ClassroomId : TypedId<ClassroomId>
{
    public ClassroomId() { }
    public ClassroomId(Guid value) : base(value) { }

    public static ClassroomId New() => new();
    public static ClassroomId Create() => new();
    public static ClassroomId Create(Guid value) => new(value);
    public static ClassroomId Create(string value) => new(Guid.Parse(value));
}
