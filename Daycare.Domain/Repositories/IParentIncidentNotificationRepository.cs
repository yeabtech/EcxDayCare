using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Queries and updates incident notifications delivered to parents.
/// </summary>
public interface IParentIncidentNotificationRepository
{
    Task<IReadOnlyList<ParentIncidentNotification>> GetByParentIdAsync(
        ParentId parentId,
        CancellationToken cancellationToken = default);

    Task EnsureForIncidentAsync(
        IncidentId incidentId,
        IReadOnlyCollection<ParentId> parentIds,
        CancellationToken cancellationToken = default);

    Task<bool> MarkAsReadAsync(
        ParentId parentId,
        IncidentId incidentId,
        CancellationToken cancellationToken = default);
}
