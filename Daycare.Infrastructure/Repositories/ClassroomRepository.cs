using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

public class ClassroomRepository : IClassroomRepository
{
    private readonly DaycareDbContext _context;

    public ClassroomRepository(DaycareDbContext context)
    {
        _context = context;
    }

    public async Task<Classroom?> GetByIdAsync(ClassroomId id, CancellationToken cancellationToken = default)
    {
        return await _context.Classrooms
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Classroom>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Classrooms
            .Include(c => c.Children)
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Classroom>> SearchAsync(string term, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        return await _context.Classrooms
            .Include(c => c.Children)
            .Where(c => EF.Functions.Like(c.Name, $"%{term}%") || (c.Description != null && EF.Functions.Like(c.Description, $"%{term}%")))
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Classroom>> GetByStatusAsync(ClassroomStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Classrooms
            .Include(c => c.Children)
            .Where(c => c.Status == status)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Classrooms.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Classroom classroom, CancellationToken cancellationToken = default)
    {
        await _context.Classrooms.AddAsync(classroom, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Classroom classroom, CancellationToken cancellationToken = default)
    {
        _context.Classrooms.Update(classroom);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Classroom classroom, CancellationToken cancellationToken = default)
    {
        _context.Classrooms.Remove(classroom);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
