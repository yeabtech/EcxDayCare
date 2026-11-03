namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for an Attendance aggregate entity.
/// </summary>
public sealed class AttendanceId : TypedId<AttendanceId>
{
    public AttendanceId() { }
    public AttendanceId(Guid value) : base(value) { }

    public static AttendanceId New() => new();
    public static AttendanceId Create() => new();
    public static AttendanceId Create(Guid value) => new(value);
    public static AttendanceId Create(string value) => new(Guid.Parse(value));
}
