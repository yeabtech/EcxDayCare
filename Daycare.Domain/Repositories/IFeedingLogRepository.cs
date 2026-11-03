using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for FeedingLog persistence.
/// </summary>
public interface IFeedingLogRepository
{
    Task<FeedingLog?> GetByIdAsync(FeedingLogId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<FeedingLog>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<FeedingLog>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(FeedingLog log, CancellationToken cancellationToken = default);
    Task UpdateAsync(FeedingLog log, CancellationToken cancellationToken = default);
    Task DeleteAsync(FeedingLog log, CancellationToken cancellationToken = default);
}
