namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for a HealthLog entity.
/// </summary>
public sealed class HealthLogId : TypedId<HealthLogId>
{
    public HealthLogId() { }
    public HealthLogId(Guid value) : base(value) { }

    public static HealthLogId New() => new(Guid.NewGuid());
    public static HealthLogId Create() => new(Guid.NewGuid());
    public static HealthLogId Create(Guid value) => new(value);
    public static HealthLogId Create(string value) => new(Guid.Parse(value));
}
