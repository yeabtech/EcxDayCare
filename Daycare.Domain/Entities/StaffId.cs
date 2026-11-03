namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed Staff ID
/// </summary>
public sealed class StaffId : TypedId<StaffId>
{
    public StaffId() { }
    public StaffId(Guid value) : base(value) { }

    public static StaffId New() => new();
    public static StaffId Create() => new();
    public static StaffId Create(Guid value) => new(value);
    public static StaffId Create(string value) => new(Guid.Parse(value));
}
