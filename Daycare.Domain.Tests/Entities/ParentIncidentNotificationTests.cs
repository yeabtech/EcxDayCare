using Daycare.Domain.Entities;

namespace Daycare.Domain.Tests.Entities;

public sealed class ParentIncidentNotificationTests
{
    [Fact]
    public void MarkAsRead_SetsReadStateAndTimestampWithoutChangingOnRepeat()
    {
        var notification = ParentIncidentNotification.Create(
            ParentId.Create(),
            IncidentId.Create());

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
