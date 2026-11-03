namespace Daycare.Domain.Entities;

/// <summary>
/// An in-app notification delivered to a parent for a daily report.
/// </summary>
public sealed class ParentNotification : BaseEntity<ParentNotificationId>
{
    public ParentId ParentId { get; private set; } = null!;

    public DailyReportId DailyReportId { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;

    public string Message { get; private set; } = string.Empty;

    public bool IsRead { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public Parent? Parent { get; private set; }

    public DailyReport? DailyReport { get; private set; }

    private ParentNotification() { }

    private ParentNotification(
        ParentNotificationId id,
        ParentId parentId,
        DailyReportId dailyReportId,
        string title,
        string message)
        : base(id)
    {
        ParentId = parentId;
        DailyReportId = dailyReportId;
        Title = title;
        Message = message;
        IsRead = false;
    }

    public static ParentNotification ForDailyReport(
        ParentId parentId,
        DailyReportId dailyReportId,
        string childName,
        DateTime reportDate)
    {
        ArgumentNullException.ThrowIfNull(parentId);
        ArgumentNullException.ThrowIfNull(dailyReportId);
        ArgumentException.ThrowIfNullOrWhiteSpace(childName);

        return new ParentNotification(
            ParentNotificationId.Create(),
            parentId,
            dailyReportId,
            "Daily report ready",
            $"{childName.Trim()}'s daily report for {reportDate:MMMM d, yyyy} is available.");
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
/// Strongly typed identifier for a parent notification.
/// </summary>
public sealed class ParentNotificationId : TypedId<ParentNotificationId>
{
    public ParentNotificationId() { }
    public ParentNotificationId(Guid value) : base(value) { }

    public static ParentNotificationId Create() => new();
    public static ParentNotificationId Create(Guid value) => new(value);
}
