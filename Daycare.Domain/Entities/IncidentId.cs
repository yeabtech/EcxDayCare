namespace Daycare.Domain.Entities;

/// <summary>
/// Strongly-typed identifier for an Incident entity.
/// </summary>
public sealed class IncidentId : TypedId<IncidentId>
{
    public IncidentId() { }
    public IncidentId(Guid value) : base(value) { }

    public static IncidentId New() => new(Guid.NewGuid());
    public static IncidentId Create() => new(Guid.NewGuid());
    public static IncidentId Create(Guid value) => new(value);
    public static IncidentId Create(string value) => new(Guid.Parse(value));
}
