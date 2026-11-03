using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of INapLogRepository.
/// </summary>
public sealed class NapLogRepository : INapLogRepository
{
    private readonly DaycareDbContext _context;

    public NapLogRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<NapLog?> GetByIdAsync(NapLogId id, CancellationToken cancellationToken = default)
        => await _context.NapLogs
            .Include(n => n.Child)
            .FirstOrDefaultAsync(n => n.Id == id, cancellationToken);

    public async Task<IEnumerable<NapLog>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var endOfDay = startOfDay.AddDays(1);

        return await _context.NapLogs
            .Include(n => n.Child)
            .Where(n => n.ChildId == childId && n.StartTime >= startOfDay && n.StartTime < endOfDay)
            .OrderBy(n => n.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<NapLog>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.NapLogs
            .Include(n => n.Child)
            .Where(n => n.ChildId == childId);

        if (startDate.HasValue)
        {
            var utcStart = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(n => n.StartTime >= utcStart);
        }

        if (endDate.HasValue)
        {
            var utcEnd = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(n => n.StartTime < utcEnd);
        }

        return await query.OrderByDescending(n => n.StartTime).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(NapLog log, CancellationToken cancellationToken = default)
    {
        await _context.NapLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(NapLog log, CancellationToken cancellationToken = default)
    {
        _context.NapLogs.Update(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(NapLog log, CancellationToken cancellationToken = default)
    {
        _context.NapLogs.Remove(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
