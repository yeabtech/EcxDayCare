using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for Child entity persistence.
/// Abstracts data access for the Child aggregate.
/// </summary>
public interface IChildRepository
{
    /// <summary>
    /// Retrieves a child by ID.
    /// </summary>
    Task<Child?> GetByIdAsync(ChildId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all children with pagination.
    /// </summary>
    Task<IReadOnlyList<Child>> GetAllAsync(
        int skip = 0,
        int take = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of children.
    /// </summary>
    Task<int> GetCountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new child to the repository.
    /// </summary>
    Task AddAsync(Child child, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing child (assumes child is already tracked).
    /// </summary>
    Task UpdateAsync(Child child, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a child by ID.
    /// </summary>
    Task DeleteAsync(ChildId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a child exists by ID.
    /// </summary>
    Task<bool> ExistsAsync(ChildId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces every row in ParentChildren for the given child with the supplied links.
    /// </summary>
    /// <param name="childId">The child whose parents are being replaced.</param>
    /// <param name="links">The new parent links (parent id + role). May be empty to unlink all parents.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ReplaceParentLinksAsync(
        ChildId childId,
        IReadOnlyCollection<ParentChildLink> links,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves every parent linked to the given child, including the stored role.
    /// </summary>
    /// <param name="childId">The child whose parent links are requested.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The parent/child links (parent id + role). May be empty.</returns>
    Task<IReadOnlyList<ParentChildLink>> GetParentLinksAsync(
        ChildId childId,
        CancellationToken cancellationToken = default);
}
