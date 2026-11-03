using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for Enrollment aggregate persistence.
/// </summary>
public interface IEnrollmentRepository
{
    /// <summary>
    /// Gets an enrollment by its strongly-typed identifier.
    /// </summary>
    Task<Enrollment?> GetByIdAsync(EnrollmentId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full enrollment history for a specific child, sorted newest first.
    /// </summary>
    Task<IEnumerable<Enrollment>> GetByChildIdAsync(ChildId childId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the currently active or pending enrollment for a child, if any.
    /// </summary>
    Task<Enrollment?> GetActiveByChildIdAsync(ChildId childId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves enrollments with pagination and optional status, program type, or classroom filters.
    /// </summary>
    Task<IEnumerable<Enrollment>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        EnrollmentStatus? status = null,
        ProgramType? programType = null,
        Guid? classroomId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total count of enrollments matching the specified filters.
    /// </summary>
    Task<int> GetTotalCountAsync(
        EnrollmentStatus? status = null,
        ProgramType? programType = null,
        Guid? classroomId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of currently active enrollments assigned to a specific classroom.
    /// </summary>
    Task<int> GetActiveCountByClassroomIdAsync(
        ClassroomId classroomId,
        EnrollmentId? excludeEnrollmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a child already has an active or pending enrollment.
    /// </summary>
    Task<bool> HasActiveEnrollmentAsync(
        ChildId childId,
        EnrollmentId? excludeEnrollmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new enrollment record.
    /// </summary>
    Task AddAsync(Enrollment enrollment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing enrollment record.
    /// </summary>
    Task UpdateAsync(Enrollment enrollment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an enrollment record.
    /// </summary>
    Task DeleteAsync(Enrollment enrollment, CancellationToken cancellationToken = default);
}
