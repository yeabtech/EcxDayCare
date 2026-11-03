using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Queries and updates notifications delivered to parents.
/// </summary>
public interface IParentNotificationRepository
{
    Task<IReadOnlyList<ParentNotification>> GetByParentIdAsync(
        ParentId parentId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkDailyReportAsReadAsync(
        ParentId parentId,
        DailyReportId dailyReportId,
        CancellationToken cancellationToken = default);
}
