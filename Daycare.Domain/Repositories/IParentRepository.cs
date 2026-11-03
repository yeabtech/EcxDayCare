using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for Parent aggregate
/// </summary>
public interface IParentRepository
{
    /// <summary>
    /// Get parent by ID
    /// </summary>
    Task<Parent?> GetByIdAsync(ParentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all parents with pagination
    /// </summary>
    Task<IEnumerable<Parent>> GetAllAsync(int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search parents by name or email
    /// </summary>
    Task<IEnumerable<Parent>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get parents by status
    /// </summary>
    Task<IEnumerable<Parent>> GetByStatusAsync(ParentStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get count of all parents
    /// </summary>
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);

    /// <summary>Checks whether another parent record already uses an email address.</summary>
    Task<bool> ExistsByEmailAsync(string email, ParentId? exceptParentId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get parents for a specific child
    /// </summary>
    Task<IEnumerable<Parent>> GetParentsByChildAsync(ChildId childId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add parent
    /// </summary>
    Task AddAsync(Parent parent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update parent
    /// </summary>
    Task UpdateAsync(Parent parent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete parent
    /// </summary>
    Task DeleteAsync(Parent parent, CancellationToken cancellationToken = default);
}
