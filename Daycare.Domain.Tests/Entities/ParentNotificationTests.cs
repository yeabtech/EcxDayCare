using Daycare.Domain.Entities;

namespace Daycare.Domain.Tests.Entities;

public sealed class ParentNotificationTests
{
    [Fact]
    public void MarkAsRead_SetsReadStateAndTimestampWithoutChangingOnRepeat()
    {
        var notification = ParentNotification.ForDailyReport(
            ParentId.Create(),
            DailyReportId.Create(),
            "Emma Watson",
            DateTime.UtcNow.Date);

        notification.MarkAsRead();
        var firstReadAt = notification.ReadAt;
        var firstUpdatedAt = notification.UpdatedAt;
        notification.MarkAsRead();

        Assert.True(notification.IsRead);
        Assert.NotNull(firstReadAt);
        Assert.Equal(firstReadAt, notification.ReadAt);
        Assert.Equal(firstUpdatedAt, notification.UpdatedAt);
    }
}
