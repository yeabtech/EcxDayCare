using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for NapLog persistence.
/// </summary>
public interface INapLogRepository
{
    Task<NapLog?> GetByIdAsync(NapLogId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<NapLog>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<NapLog>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(NapLog log, CancellationToken cancellationToken = default);
    Task UpdateAsync(NapLog log, CancellationToken cancellationToken = default);
    Task DeleteAsync(NapLog log, CancellationToken cancellationToken = default);
}
