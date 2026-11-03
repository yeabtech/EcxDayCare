using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for ActivityLog persistence.
/// </summary>
public interface IActivityLogRepository
{
    Task<ActivityLog?> GetByIdAsync(ActivityLogId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<ActivityLog>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<ActivityLog>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(ActivityLog log, CancellationToken cancellationToken = default);
    Task UpdateAsync(ActivityLog log, CancellationToken cancellationToken = default);
    Task DeleteAsync(ActivityLog log, CancellationToken cancellationToken = default);
}
