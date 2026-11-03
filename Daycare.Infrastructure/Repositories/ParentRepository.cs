using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Parent aggregate
/// </summary>
public class ParentRepository : IParentRepository
{
    private readonly DaycareDbContext _context;

    public ParentRepository(DaycareDbContext context)
    {
        _context = context;
    }

    public async Task<Parent?> GetByIdAsync(ParentId id, CancellationToken cancellationToken = default)
    {
        return await _context.Parents
            .Include(p => p.ParentChildren)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Parent>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;

        return await _context.Parents
            .Include(p => p.ParentChildren)
            .OrderBy(p => p.Name.FirstName)
            .ThenBy(p => p.Name.LastName)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Parent>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var skip = (pageNumber - 1) * pageSize;
        var term = searchTerm.ToLower();

        return await _context.Parents
            .Include(p => p.ParentChildren)
            .Where(p => 
                EF.Functions.Like(p.Name.FirstName, $"%{term}%") ||
                EF.Functions.Like(p.Name.LastName, $"%{term}%") ||
                EF.Functions.Like(p.ContactInfo.Email, $"%{term}%"))
            .OrderBy(p => p.Name.FirstName)
            .ThenBy(p => p.Name.LastName)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Parent>> GetByStatusAsync(ParentStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Parents
            .Include(p => p.ParentChildren)
            .Where(p => p.Status == status)
            .OrderBy(p => p.Name.FirstName)
            .ThenBy(p => p.Name.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Parents.CountAsync(cancellationToken);
    }

    public Task<bool> ExistsByEmailAsync(string email, ParentId? exceptParentId = null, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var query = _context.Parents
            .Where(parent => parent.ContactInfo.Email.ToLower() == normalizedEmail);

        if (exceptParentId is not null)
            query = query.Where(parent => parent.Id != exceptParentId);

        return query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Parent>> GetParentsByChildAsync(ChildId childId, CancellationToken cancellationToken = default)
    {
        return await _context.Parents
            .Include(p => p.ParentChildren)
            .Where(p => p.ParentChildren.Any(pc => pc.ChildId == childId))
            .OrderBy(p => p.Name.FirstName)
            .ThenBy(p => p.Name.LastName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Parent parent, CancellationToken cancellationToken = default)
    {
        await _context.Parents.AddAsync(parent, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Parent parent, CancellationToken cancellationToken = default)
    {
        _context.Parents.Update(parent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Parent parent, CancellationToken cancellationToken = default)
    {
        _context.Parents.Remove(parent);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
