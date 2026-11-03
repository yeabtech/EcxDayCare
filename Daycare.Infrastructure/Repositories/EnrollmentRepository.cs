using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of IEnrollmentRepository.
/// </summary>
public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly DaycareDbContext _context;

    public EnrollmentRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Enrollment?> GetByIdAsync(EnrollmentId id, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Child)
            .Include(e => e.Classroom)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetByChildIdAsync(ChildId childId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Classroom)
            .Where(e => e.ChildId == childId)
            .OrderByDescending(e => e.StartDate)
            .ThenByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Enrollment?> GetActiveByChildIdAsync(ChildId childId, CancellationToken cancellationToken = default)
    {
        return await _context.Enrollments
            .Include(e => e.Classroom)
            .Where(e => e.ChildId == childId && (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending))
            .OrderByDescending(e => e.StartDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<Enrollment>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        EnrollmentStatus? status = null,
        ProgramType? programType = null,
        Guid? classroomId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Enrollments
            .Include(e => e.Child)
            .Include(e => e.Classroom)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (programType.HasValue)
        {
            query = query.Where(e => e.ProgramType == programType.Value);
        }

        if (classroomId.HasValue)
        {
            var cid = ClassroomId.Create(classroomId.Value);
            query = query.Where(e => e.ClassroomId == cid);
        }

        return await query
            .OrderByDescending(e => e.StartDate)
            .ThenByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        EnrollmentStatus? status = null,
        ProgramType? programType = null,
        Guid? classroomId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Enrollments.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        if (programType.HasValue)
        {
            query = query.Where(e => e.ProgramType == programType.Value);
        }

        if (classroomId.HasValue)
        {
            var cid = ClassroomId.Create(classroomId.Value);
            query = query.Where(e => e.ClassroomId == cid);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetActiveCountByClassroomIdAsync(
        ClassroomId classroomId,
        EnrollmentId? excludeEnrollmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Enrollments
            .Where(e => e.ClassroomId == classroomId &&
                        (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending));

        if (excludeEnrollmentId != null)
        {
            query = query.Where(e => e.Id != excludeEnrollmentId);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<bool> HasActiveEnrollmentAsync(
        ChildId childId,
        EnrollmentId? excludeEnrollmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Enrollments
            .Where(e => e.ChildId == childId &&
                        (e.Status == EnrollmentStatus.Active || e.Status == EnrollmentStatus.Pending));

        if (excludeEnrollmentId != null)
        {
            query = query.Where(e => e.Id != excludeEnrollmentId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        await _context.Enrollments.AddAsync(enrollment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        _context.Enrollments.Update(enrollment);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Enrollment enrollment, CancellationToken cancellationToken = default)
    {
        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
