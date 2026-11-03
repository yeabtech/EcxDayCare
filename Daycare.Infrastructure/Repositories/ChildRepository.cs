using Microsoft.EntityFrameworkCore;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Child entity persistence.
/// Provides CRUD operations and queries against PostgreSQL via EF Core.
/// </summary>
public sealed class ChildRepository : IChildRepository
{
    private readonly DaycareDbContext _context;

    public ChildRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Child?> GetByIdAsync(ChildId id, CancellationToken cancellationToken = default)
    {
        return await _context.Children
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Child>> GetAllAsync(
        int skip = 0,
        int take = 10,
        CancellationToken cancellationToken = default)
    {
        return await _context.Children
            .OrderBy(c => c.Name.FirstName)
            .ThenBy(c => c.Name.LastName)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetCountAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Children.CountAsync(cancellationToken);
    }

    public async Task AddAsync(Child child, CancellationToken cancellationToken = default)
    {
        await _context.Children.AddAsync(child, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Child child, CancellationToken cancellationToken = default)
    {
        _context.Children.Update(child);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(ChildId id, CancellationToken cancellationToken = default)
    {
        var child = await _context.Children.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
        if (child is not null)
        {
            _context.Children.Remove(child);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> ExistsAsync(ChildId id, CancellationToken cancellationToken = default)
    {
        return await _context.Children.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task ReplaceParentLinksAsync(
        ChildId childId,
        IReadOnlyCollection<ParentChildLink> links,
        CancellationToken cancellationToken = default)
    {
        var existingLinks = await _context.ParentChildren
            .Where(pc => pc.ChildId == childId)
            .ToListAsync(cancellationToken);

        if (existingLinks.Count > 0)
            _context.ParentChildren.RemoveRange(existingLinks);

        foreach (var link in links)
        {
            _context.ParentChildren.Add(new ParentChild
            {
                ParentId = link.ParentId,
                ChildId = childId,
                Role = link.Role
            });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ParentChildLink>> GetParentLinksAsync(
        ChildId childId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ParentChildren
            .Where(pc => pc.ChildId == childId)
            .Select(pc => new ParentChildLink(pc.ParentId, pc.Role))
            .ToListAsync(cancellationToken);
    }
}
