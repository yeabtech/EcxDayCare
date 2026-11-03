namespace Daycare.Domain.Entities;

/// <summary>
/// Tracks whether a parent has opened an incident report notification.
/// </summary>
public sealed class ParentIncidentNotification : BaseEntity<ParentIncidentNotificationId>
{
    public ParentId ParentId { get; private set; } = null!;
    public IncidentId IncidentId { get; private set; } = null!;
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public Parent? Parent { get; private set; }
    public Incident? Incident { get; private set; }

    private ParentIncidentNotification() { }

    private ParentIncidentNotification(ParentId parentId, IncidentId incidentId)
        : base(ParentIncidentNotificationId.Create())
    {
        ParentId = parentId;
        IncidentId = incidentId;
    }

    public static ParentIncidentNotification Create(ParentId parentId, IncidentId incidentId)
    {
        ArgumentNullException.ThrowIfNull(parentId);
        ArgumentNullException.ThrowIfNull(incidentId);
        return new ParentIncidentNotification(parentId, incidentId);
    }

    public void MarkAsRead()
    {
        if (IsRead)
            return;

        IsRead = true;
        ReadAt = DateTime.UtcNow;
        UpdateTimestamp();
    }
}

/// <summary>
/// Strongly typed identifier for a parent incident notification.
/// </summary>
public sealed class ParentIncidentNotificationId : TypedId<ParentIncidentNotificationId>
{
    public ParentIncidentNotificationId() { }
    public ParentIncidentNotificationId(Guid value) : base(value) { }

    public static ParentIncidentNotificationId Create() => new();
    public static ParentIncidentNotificationId Create(Guid value) => new(value);
}
