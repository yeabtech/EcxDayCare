namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for a FeedingLog entity.
/// </summary>
public sealed class FeedingLogId : TypedId<FeedingLogId>
{
    public FeedingLogId() { }
    public FeedingLogId(Guid value) : base(value) { }

    public static FeedingLogId New() => new(Guid.NewGuid());
    public static FeedingLogId Create() => new(Guid.NewGuid());
    public static FeedingLogId Create(Guid value) => new(value);
    public static FeedingLogId Create(string value) => new(Guid.Parse(value));
}
