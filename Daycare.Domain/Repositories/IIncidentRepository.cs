using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for Incident persistence.
/// </summary>
public interface IIncidentRepository
{
    Task<Incident?> GetByIdAsync(IncidentId id, CancellationToken cancellationToken = default);

    Task<IEnumerable<Incident>> GetByChildIdAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Incident>> GetByChildAndDateAsync(
        ChildId childId,
        DateTime date,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Incident>> GetByInjuryTypeAsync(
        InjuryType injuryType,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Incident>> GetNotSignedIncidentsAsync(
        ChildId childId,
        CancellationToken cancellationToken = default);

    Task AddAsync(Incident incident, CancellationToken cancellationToken = default);
    Task UpdateAsync(Incident incident, CancellationToken cancellationToken = default);
    Task DeleteAsync(Incident incident, CancellationToken cancellationToken = default);
}
