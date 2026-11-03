using Daycare.Domain.Entities;

namespace Daycare.Domain.Repositories;

/// <summary>
/// Repository interface for Attendance aggregate persistence and domain queries.
/// </summary>
public interface IAttendanceRepository
{
    /// <summary>
    /// Retrieves an attendance record by its strongly typed identifier.
    /// </summary>
    Task<Attendance?> GetByIdAsync(AttendanceId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the attendance record for a specific child on a given calendar date (UTC).
    /// </summary>
    Task<Attendance?> GetTodayByChildIdAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a child has an active check-in on a given date (checked in and not yet checked out).
    /// </summary>
    Task<bool> HasActiveCheckInAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any attendance record exists for the given child on that date.
    /// Used to enforce business rules preventing invalid duplicate check-ins.
    /// </summary>
    Task<bool> HasAttendanceForDateAsync(ChildId childId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all attendance records for a given date with optional classroom and status filters.
    /// </summary>
    Task<IEnumerable<Attendance>> GetTodayAttendanceAsync(
        DateTime date,
        Guid? classroomId = null,
        AttendanceStatus? status = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves paginated historical attendance records for a specific child.
    /// </summary>
    Task<IEnumerable<Attendance>> GetChildAttendanceHistoryAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of attendance records for a child within an optional date range.
    /// </summary>
    Task<int> GetChildAttendanceCountAsync(
        ChildId childId,
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new attendance record.
    /// </summary>
    Task AddAsync(Attendance attendance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing attendance record.
    /// </summary>
    Task UpdateAsync(Attendance attendance, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an attendance record.
    /// </summary>
    Task DeleteAsync(Attendance attendance, CancellationToken cancellationToken = default);
}
