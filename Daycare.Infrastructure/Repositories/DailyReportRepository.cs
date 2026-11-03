using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IDailyReportRepository.
/// </summary>
public class DailyReportRepository : IDailyReportRepository
{
    private readonly DaycareDbContext _context;

    public DailyReportRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<DailyReport?> GetByIdAsync(DailyReportId id, CancellationToken cancellationToken = default)
    {
        return await _context.DailyReports
            .Include(r => r.Child)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<DailyReport?> GetByChildAndDateAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _context.DailyReports
            .Include(r => r.Child)
            .FirstOrDefaultAsync(r => r.ChildId == childId && r.Date == utcDate, cancellationToken);
    }

    public async Task<IEnumerable<DailyReport>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DailyReports
            .Include(r => r.Child)
            .Where(r => r.ChildId == childId);

        if (startDate.HasValue)
        {
            var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.Date >= start);
        }

        if (endDate.HasValue)
        {
            var end = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.Date <= end);
        }

        var skip = Math.Max(0, (page - 1) * pageSize);

        return await query
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DailyReports.Where(r => r.ChildId == childId);

        if (startDate.HasValue)
        {
            var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.Date >= start);
        }

        if (endDate.HasValue)
        {
            var end = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(r => r.Date <= end);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<DailyReport>> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _context.DailyReports
            .Include(r => r.Child)
            .Where(r => r.Date == utcDate)
            .OrderBy(r => r.Child != null ? r.Child.Name.LastName : "")
            .ThenBy(r => r.Child != null ? r.Child.Name.FirstName : "")
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default)
    {
        var utcDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _context.DailyReports
            .AnyAsync(r => r.ChildId == childId && r.Date == utcDate, cancellationToken);
    }

    public async Task AddAsync(DailyReport report, CancellationToken cancellationToken = default)
    {
        await _context.DailyReports.AddAsync(report, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(DailyReport report, CancellationToken cancellationToken = default)
    {
        _context.DailyReports.Update(report);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkSentToParentAsync(
        DailyReport report,
        IReadOnlyCollection<ParentNotification> notifications,
        CancellationToken cancellationToken = default)
    {
        _context.DailyReports.Update(report);
        await _context.ParentNotifications.AddRangeAsync(notifications, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(DailyReport report, CancellationToken cancellationToken = default)
    {
        _context.DailyReports.Remove(report);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
