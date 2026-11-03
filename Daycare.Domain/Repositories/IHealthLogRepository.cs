using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for HealthLog persistence.
/// </summary>
public interface IHealthLogRepository
{
    Task<HealthLog?> GetByIdAsync(HealthLogId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<HealthLog>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<HealthLog>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<HealthLog>> GetByChildAndEventTypeAsync(
        ChildId childId,
        HealthEventType eventType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(HealthLog log, CancellationToken cancellationToken = default);
    Task UpdateAsync(HealthLog log, CancellationToken cancellationToken = default);
    Task DeleteAsync(HealthLog log, CancellationToken cancellationToken = default);
}
