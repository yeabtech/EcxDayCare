namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for an ActivityLog entity.
/// </summary>
public sealed class ActivityLogId : TypedId<ActivityLogId>
{
    public ActivityLogId() { }
    public ActivityLogId(Guid value) : base(value) { }

    public static ActivityLogId New() => new(Guid.NewGuid());
    public static ActivityLogId Create() => new(Guid.NewGuid());
    public static ActivityLogId Create(Guid value) => new(value);
    public static ActivityLogId Create(string value) => new(Guid.Parse(value));
}
