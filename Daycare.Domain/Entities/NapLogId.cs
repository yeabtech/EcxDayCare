namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for a NapLog entity.
/// </summary>
public sealed class NapLogId : TypedId<NapLogId>
{
    public NapLogId() { }
    public NapLogId(Guid value) : base(value) { }

    public static NapLogId New() => new(Guid.NewGuid());
    public static NapLogId Create() => new(Guid.NewGuid());
    public static NapLogId Create(Guid value) => new(value);
    public static NapLogId Create(string value) => new(Guid.Parse(value));
}
