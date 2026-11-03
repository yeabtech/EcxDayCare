using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Daycare.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation for parent incident notification queries and read state.
/// </summary>
public sealed class ParentIncidentNotificationRepository : IParentIncidentNotificationRepository
{
    private readonly DaycareDbContext _context;

    public ParentIncidentNotificationRepository(DaycareDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IReadOnlyList<ParentIncidentNotification>> GetByParentIdAsync(
        ParentId parentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ParentIncidentNotifications
            .AsNoTracking()
            .Include(notification => notification.Incident)
            .ThenInclude(incident => incident!.Child)
            .Where(notification => notification.ParentId == parentId)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task EnsureForIncidentAsync(
        IncidentId incidentId,
        IReadOnlyCollection<ParentId> parentIds,
        CancellationToken cancellationToken = default)
    {
        if (parentIds.Count == 0)
            return;

        var existingParentIds = await _context.ParentIncidentNotifications
            .Where(notification => notification.IncidentId == incidentId)
            .Select(notification => notification.ParentId)
            .ToListAsync(cancellationToken);
        var existing = existingParentIds.ToHashSet();
        var missingParentIds = parentIds
            .Distinct()
            .Where(parentId => !existing.Contains(parentId))
            .ToArray();

        foreach (var parentId in missingParentIds)
            _context.ParentIncidentNotifications.Add(ParentIncidentNotification.Create(parentId, incidentId));

        if (missingParentIds.Length > 0)
            await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> MarkAsReadAsync(
        ParentId parentId,
        IncidentId incidentId,
        CancellationToken cancellationToken = default)
    {
        var notification = await _context.ParentIncidentNotifications
            .FirstOrDefaultAsync(
                item => item.ParentId == parentId && item.IncidentId == incidentId,
                cancellationToken);
        if (notification is null)
            return false;

        notification.MarkAsRead();
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
