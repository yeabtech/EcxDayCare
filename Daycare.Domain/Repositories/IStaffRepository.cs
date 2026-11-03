using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for Staff aggregate
/// </summary>
public interface IStaffRepository
{
    /// <summary>
    /// Gets staff member by ID
    /// </summary>
    Task<Staff?> GetByIdAsync(StaffId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a staff member by their unique email address.
    /// </summary>
    Task<Staff?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all staff members with pagination and optional filters
    /// </summary>
    Task<IEnumerable<Staff>> GetAllAsync(
        int page = 1,
        int pageSize = 10,
        Guid? classroomId = null,
        StaffRole? role = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches staff members by name, email, phone, role, or certifications
    /// </summary>
    Task<IEnumerable<Staff>> SearchAsync(
        string term,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets staff members assigned to a specific classroom
    /// </summary>
    Task<IEnumerable<Staff>> GetByClassroomIdAsync(ClassroomId classroomId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets staff members by role
    /// </summary>
    Task<IEnumerable<Staff>> GetByRoleAsync(StaffRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count of staff members matching optional filters
    /// </summary>
    Task<int> GetTotalCountAsync(
        Guid? classroomId = null,
        StaffRole? role = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets total count of staff members matching search term
    /// </summary>
    Task<int> GetSearchCountAsync(string term, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a staff member with given email already exists
    /// </summary>
    Task<bool> ExistsByEmailAsync(string email, StaffId? excludeStaffId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new staff member
    /// </summary>
    Task AddAsync(Staff staff, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing staff member
    /// </summary>
    Task UpdateAsync(Staff staff, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a staff member
    /// </summary>
    Task DeleteAsync(Staff staff, CancellationToken cancellationToken = default);
}
