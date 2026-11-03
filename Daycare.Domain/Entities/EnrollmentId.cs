namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for an Enrollment entity.
/// </summary>
public sealed class EnrollmentId : TypedId<EnrollmentId>
{
    public EnrollmentId() { }
    public EnrollmentId(Guid value) : base(value) { }

    public static EnrollmentId New() => new();
    public static EnrollmentId Create() => new();
    public static EnrollmentId Create(Guid value) => new(value);
    public static EnrollmentId Create(string value) => new(Guid.Parse(value));
}
