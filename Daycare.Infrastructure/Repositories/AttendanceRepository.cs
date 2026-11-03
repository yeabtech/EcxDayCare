using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IAttendanceRepository.
/// </summary>
public class AttendanceRepository : IAttendanceRepository
{
    private readonly DaycareDbContext _context;

    public AttendanceRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Attendance?> GetByIdAsync(AttendanceId id, CancellationToken cancellationToken = default)
    {
        return await _context.Attendances
            .Include(a => a.Child)
            .Include(a => a.Classroom)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Attendance?> GetTodayByChildIdAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default)
    {
        var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _context.Attendances
            .Include(a => a.Child)
            .Include(a => a.Classroom)
            .FirstOrDefaultAsync(a => a.ChildId == childId && a.Date == targetDate, cancellationToken);
    }

    public async Task<bool> HasActiveCheckInAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default)
    {
        var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _context.Attendances
            .AnyAsync(a => a.ChildId == childId && a.Date == targetDate && a.CheckInTime != null && a.CheckOutTime == null, cancellationToken);
    }

    public async Task<bool> HasAttendanceForDateAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default)
    {
        var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return await _context.Attendances
            .AnyAsync(a => a.ChildId == childId && a.Date == targetDate, cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetTodayAttendanceAsync(
        DateTime date,
        Guid? classroomId = null,
        AttendanceStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var targetDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var query = _context.Attendances
            .Include(a => a.Child)
            .Include(a => a.Classroom)
            .Where(a => a.Date == targetDate);

        if (classroomId.HasValue && classroomId.Value != Guid.Empty)
        {
            var cid = ClassroomId.Create(classroomId.Value);
            query = query.Where(a => a.ClassroomId == cid);
        }

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        return await query
            .OrderBy(a => a.Child != null ? a.Child.Name.LastName : "")
            .ThenBy(a => a.Child != null ? a.Child.Name.FirstName : "")
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Attendance>> GetChildAttendanceHistoryAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Attendances
            .Include(a => a.Child)
            .Include(a => a.Classroom)
            .Where(a => a.ChildId == childId);

        if (startDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(a => a.Date >= startUtc);
        }

        if (endDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(a => a.Date <= endUtc);
        }

        var skip = Math.Max(0, (page - 1) * pageSize);

        return await query
            .OrderByDescending(a => a.Date)
            .ThenByDescending(a => a.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetChildAttendanceCountAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Attendances
            .Where(a => a.ChildId == childId);

        if (startDate.HasValue)
        {
            var startUtc = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(a => a.Date >= startUtc);
        }

        if (endDate.HasValue)
        {
            var endUtc = DateTime.SpecifyKind(endDate.Value.Date, DateTimeKind.Utc);
            query = query.Where(a => a.Date <= endUtc);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        await _context.Attendances.AddAsync(attendance, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        _context.Attendances.Update(attendance);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Attendance attendance, CancellationToken cancellationToken = default)
    {
        _context.Attendances.Remove(attendance);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
