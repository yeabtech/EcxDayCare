using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for HealthLog persistence.
/// </summary>
public sealed class HealthLogRepository : IHealthLogRepository
{
    private readonly DaycareDbContext _context;

    public HealthLogRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<HealthLog?> GetByIdAsync(HealthLogId id, CancellationToken cancellationToken = default)
    {
        return await _context.HealthLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<HealthLog>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        return await _context.HealthLogs
            .AsNoTracking()
            .Where(h => h.ChildId == childId && h.Time >= startOfDay && h.Time <= endOfDay)
            .OrderByDescending(h => h.Time)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HealthLog>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.HealthLogs
            .AsNoTracking()
            .Where(h => h.ChildId == childId);

        if (startDate.HasValue)
            query = query.Where(h => h.Time >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(h => h.Time <= endDate.Value);

        return await query.OrderByDescending(h => h.Time).ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<HealthLog>> GetByChildAndEventTypeAsync(
        ChildId childId,
        HealthEventType eventType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.HealthLogs
            .AsNoTracking()
            .Where(h => h.ChildId == childId && h.EventType == eventType);

        if (startDate.HasValue)
            query = query.Where(h => h.Time >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(h => h.Time <= endDate.Value);

        return await query.OrderByDescending(h => h.Time).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(HealthLog log, CancellationToken cancellationToken = default)
    {
        await _context.HealthLogs.AddAsync(log, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(HealthLog log, CancellationToken cancellationToken = default)
    {
        _context.HealthLogs.Update(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(HealthLog log, CancellationToken cancellationToken = default)
    {
        _context.HealthLogs.Remove(log);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
