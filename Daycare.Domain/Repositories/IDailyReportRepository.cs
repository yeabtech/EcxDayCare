using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Contract for persisting and querying DailyReport aggregates.
/// </summary>
public interface IDailyReportRepository
{
    /// <summary>Gets a report by its primary key.</summary>
    Task<DailyReport?> GetByIdAsync(DailyReportId id, CancellationToken cancellationToken = default);

    /// <summary>Gets a report for a specific child on a specific date.</summary>
    Task<DailyReport?> GetByChildAndDateAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns paginated reports for a child, optionally filtered by date range.
    /// </summary>
    Task<IEnumerable<DailyReport>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 30,
        CancellationToken cancellationToken = default);

    /// <summary>Returns total count of reports for a child in the given date range.</summary>
    Task<int> GetCountByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all reports authored on a specific date (daily feed for staff).
    /// </summary>
    Task<IEnumerable<DailyReport>> GetByDateAsync(
        DateTime date,
        CancellationToken cancellationToken = default);

    /// <summary>Checks whether a report already exists for the given child + date.</summary>
    Task<bool> ExistsAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default);

    Task AddAsync(DailyReport report, CancellationToken cancellationToken = default);
    Task UpdateAsync(DailyReport report, CancellationToken cancellationToken = default);
    Task MarkSentToParentAsync(
        DailyReport report,
        IReadOnlyCollection<ParentNotification> notifications,
        CancellationToken cancellationToken = default);
    Task DeleteAsync(DailyReport report, CancellationToken cancellationToken = default);
}
