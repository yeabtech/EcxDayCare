using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IActivityLogRepository.
/// </summary>
public sealed class ActivityLogRepository : IActivityLogRepository
{
    private readonly DaycareDbContext _context;

    public ActivityLogRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<ActivityLog?> GetByIdAsync(ActivityLogId id, CancellationToken cancellationToken = default)
        => await _context.ActivityLogs
            .Include(a => a.Child)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IEnumerable<ActivityLog>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        return await _context.ActivityLogs
            .Include(a => a.Child)
            .Where(a => a.ChildId == childId && a.Time >= startOfDay && a.Time < endOfDay)
            .OrderBy(a => a.Time)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ActivityLog>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.ActivityLogs
            .Include(a => a.Child)
            .Where(a => a.ChildId == childId);

        if (startDate.HasValue)
        {
            var utcStart = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(a => a.Time >= utcStart);
        }

        if (endDate.HasValue)
        {
            var utcEnd = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(a => a.Time < utcEnd);
        }

        return await query.OrderByDescending(a => a.Time).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        await _context.ActivityLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        _context.ActivityLogs.Update(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ActivityLog log, CancellationToken cancellationToken = default)
    {
        _context.ActivityLogs.Remove(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
