using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation for parent notification queries and read state.
/// </summary>
public sealed class ParentNotificationRepository : IParentNotificationRepository
{
    private readonly DaycareDbContext _context;

    public ParentNotificationRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<ParentNotification>> GetByParentIdAsync(
        ParentId parentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ParentNotifications
            .AsNoTracking()
            .Include(notification => notification.DailyReport)
            .ThenInclude(report => report!.Child)
            .Where(notification => notification.ParentId == parentId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> MarkDailyReportAsReadAsync(
        ParentId parentId,
        DailyReportId dailyReportId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.ParentNotifications
            .FirstOrDefaultAsync(
                item => item.ParentId == parentId && item.DailyReportId == dailyReportId,
                cancellationToken);
        if (notification is null)
            return false;

        notification.MarkAsRead();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
