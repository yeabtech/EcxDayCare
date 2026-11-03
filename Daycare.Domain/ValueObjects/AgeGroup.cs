using Daycare.Domain.Results;

namespace Daycare.Domain.ValueObjects;

/// <summary>
/// Value object representing age group constraints
/// </summary>
public sealed class AgeGroup : IEquatable<AgeGroup>
{
    public int MinAgeMonths { get; }
    public int MaxAgeMonths { get; }

    // Parameterless constructor for EF Core
    private AgeGroup() { }

    private AgeGroup(int minAgeMonths, int maxAgeMonths)
    {
        MinAgeMonths = minAgeMonths;
        MaxAgeMonths = maxAgeMonths;
    }

    public static Result<AgeGroup> Create(int minAgeMonths, int maxAgeMonths)
    {
        if (minAgeMonths < 0)
            return Result<AgeGroup>.Failure("Minimum age cannot be negative");

        if (maxAgeMonths < 0)
            return Result<AgeGroup>.Failure("Maximum age cannot be negative");

        if (minAgeMonths > maxAgeMonths)
            return Result<AgeGroup>.Failure("Minimum age cannot be greater than maximum age");

        if (maxAgeMonths - minAgeMonths < 6)
            return Result<AgeGroup>.Failure("Age range must be at least 6 months");

        if (maxAgeMonths > 84) // 7 years
            return Result<AgeGroup>.Failure("Maximum age cannot exceed 84 months (7 years)");

        return Result<AgeGroup>.Success(new AgeGroup(minAgeMonths, maxAgeMonths));
    }

    public bool IsAgeInRange(int ageMonths) => ageMonths >= MinAgeMonths && ageMonths <= MaxAgeMonths;

    public bool Equals(AgeGroup? other)
    {
        if (other is null) return false;
        return MinAgeMonths == other.MinAgeMonths && MaxAgeMonths == other.MaxAgeMonths;
    }

    public override bool Equals(object? obj) => Equals(obj as AgeGroup);

    public override int GetHashCode() => HashCode.Combine(MinAgeMonths, MaxAgeMonths);

    public override string ToString() => $"{MinAgeMonths}-{MaxAgeMonths} months";
}

/// <summary>
/// Value object representing classroom capacity
/// </summary>
public sealed class ClassroomCapacity : IEquatable<ClassroomCapacity>
{
    public int MaxCapacity { get; }
    public const int MinCapacity = 1;
    public const int AbsoluteMaxCapacity = 30;

    // Parameterless constructor for EF Core
    private ClassroomCapacity() { }

    private ClassroomCapacity(int maxCapacity)
    {
        MaxCapacity = maxCapacity;
    }

    public static Result<ClassroomCapacity> Create(int maxCapacity)
    {
        if (maxCapacity < MinCapacity)
            return Result<ClassroomCapacity>.Failure($"Capacity must be at least {MinCapacity}");

        if (maxCapacity > AbsoluteMaxCapacity)
            return Result<ClassroomCapacity>.Failure($"Capacity cannot exceed {AbsoluteMaxCapacity}");

        return Result<ClassroomCapacity>.Success(new ClassroomCapacity(maxCapacity));
    }

    public bool IsCapacityFull(int currentCount) => currentCount >= MaxCapacity;

    public int GetAvailableSpots(int currentCount) => Math.Max(0, MaxCapacity - currentCount);

    public bool Equals(ClassroomCapacity? other)
    {
        if (other is null) return false;
        return MaxCapacity == other.MaxCapacity;
    }

    public override bool Equals(object? obj) => Equals(obj as ClassroomCapacity);

    public override int GetHashCode() => MaxCapacity.GetHashCode();

    public override string ToString() => $"Capacity: {MaxCapacity}";
}
