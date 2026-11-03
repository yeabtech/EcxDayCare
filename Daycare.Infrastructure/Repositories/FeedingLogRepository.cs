using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IFeedingLogRepository.
/// </summary>
public sealed class FeedingLogRepository : IFeedingLogRepository
{
    private readonly DaycareDbContext _context;

    public FeedingLogRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<FeedingLog?> GetByIdAsync(FeedingLogId id, CancellationToken cancellationToken = default)
        => await _context.FeedingLogs
            .Include(f => f.Child)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IEnumerable<FeedingLog>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        return await _context.FeedingLogs
            .Include(f => f.Child)
            .Where(f => f.ChildId == childId && f.Time >= startOfDay && f.Time < endOfDay)
            .OrderBy(f => f.Time)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<FeedingLog>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.FeedingLogs
            .Include(f => f.Child)
            .Where(f => f.ChildId == childId);

        if (startDate.HasValue)
        {
            var utcStart = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(f => f.Time >= utcStart);
        }

        if (endDate.HasValue)
        {
            var utcEnd = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(f => f.Time < utcEnd);
        }

        return await query.OrderByDescending(f => f.Time).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(FeedingLog log, CancellationToken cancellationToken = default)
    {
        await _context.FeedingLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(FeedingLog log, CancellationToken cancellationToken = default)
    {
        _context.FeedingLogs.Update(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(FeedingLog log, CancellationToken cancellationToken = default)
    {
        _context.FeedingLogs.Remove(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
