using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core repository implementation for Staff entity
/// </summary>
public class StaffRepository : IStaffRepository
{
    private readonly DaycareDbContext _context;

    public StaffRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Staff?> GetByIdAsync(StaffId id, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Classroom)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Staff?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        var normalizedEmail = email.Trim().ToLower();
        return await _context.Staff
            .FirstOrDefaultAsync(staff => staff.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        Guid? classroomId = null,
        StaffRole? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Staff
            .Include(s => s.Classroom)
            .AsQueryable();

        if (classroomId.HasValue)
        {
            var cid = ClassroomId.Create(classroomId.Value);
            query = query.Where(s => s.ClassroomId == cid);
        }

        if (role.HasValue)
        {
            query = query.Where(s => s.Role == role.Value);
        }

        return await query
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Staff>> SearchAsync(
        string term,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pattern = $"%{term}%";

        return await _context.Staff
            .Include(s => s.Classroom)
            .Where(s =>
                EF.Functions.ILike(s.FirstName, pattern) ||
                EF.Functions.ILike(s.LastName, pattern) ||
                EF.Functions.ILike(s.Email, pattern) ||
                EF.Functions.ILike(s.Phone, pattern) ||
                (s.Certifications != null && EF.Functions.ILike(s.Certifications, pattern)))
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetByClassroomIdAsync(ClassroomId classroomId, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Classroom)
            .Where(s => s.ClassroomId == classroomId)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Staff>> GetByRoleAsync(StaffRole role, CancellationToken cancellationToken = default)
    {
        return await _context.Staff
            .Include(s => s.Classroom)
            .Where(s => s.Role == role)
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(
        Guid? classroomId = null,
        StaffRole? role = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Staff.AsQueryable();

        if (classroomId.HasValue)
        {
            var cid = ClassroomId.Create(classroomId.Value);
            query = query.Where(s => s.ClassroomId == cid);
        }

        if (role.HasValue)
        {
            query = query.Where(s => s.Role == role.Value);
        }

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> GetSearchCountAsync(string term, CancellationToken cancellationToken = default)
    {
        var pattern = $"%{term}%";

        return await _context.Staff
            .Where(s =>
                EF.Functions.ILike(s.FirstName, pattern) ||
                EF.Functions.ILike(s.LastName, pattern) ||
                EF.Functions.ILike(s.Email, pattern) ||
                EF.Functions.ILike(s.Phone, pattern) ||
                (s.Certifications != null && EF.Functions.ILike(s.Certifications, pattern)))
            .CountAsync(cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, StaffId? excludeStaffId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = _context.Staff.Where(s => s.Email == normalizedEmail);

        if (excludeStaffId != null)
        {
            query = query.Where(s => s.Id != excludeStaffId);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Staff staff, CancellationToken cancellationToken = default)
    {
        await _context.Staff.AddAsync(staff, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Staff staff, CancellationToken cancellationToken = default)
    {
        _context.Staff.Update(staff);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Staff staff, CancellationToken cancellationToken = default)
    {
        _context.Staff.Remove(staff);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
