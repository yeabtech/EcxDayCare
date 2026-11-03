namespace Daycare.Application.DTOs;

/// <summary>
/// Data Transfer Object representing an Attendance record.
/// </summary>
public sealed record AttendanceDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    Guid? ClassroomId,
    string? ClassroomName,
    DateTime Date,
    DateTime? CheckInTime,
    DateTime? CheckOutTime,
    string Status,
    string? DroppedOffBy,
    string? PickedUpBy,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    Guid? StaffId = null,
    Guid? CheckOutStaffId = null);

/// <summary>
/// Request DTO for checking in a child.
/// </summary>
public sealed record CheckInRequestDto(
    Guid ChildId,
    DateTime? CheckInTime = null,
    Guid? ClassroomId = null,
    string? DroppedOffBy = null,
    string? Notes = null,
    string? Status = null);

/// <summary>
/// Request DTO for checking out a child.
/// </summary>
public sealed record CheckOutRequestDto(
    Guid? ChildId = null,
    DateTime? CheckOutTime = null,
    string? PickedUpBy = null,
    string? Notes = null,
    bool? IsEarlyPickup = null);

/// <summary>
/// Request DTO for marking a child absent.
/// </summary>
public sealed record MarkAbsentRequestDto(
    Guid ChildId,
    DateTime? Date = null,
    string? Notes = null);

/// <summary>
/// Summary DTO for daily daycare attendance dashboard.
/// </summary>
public sealed record DailyAttendanceSummaryDto(
    DateTime Date,
    int TotalEnrolled,
    int TotalPresent,
    int TotalLate,
    int TotalAbsent,
    int TotalEarlyPickup,
    int TotalCurrentlyCheckedIn,
    IReadOnlyList<AttendanceDto> Items);

/// <summary>
/// Detailed attendance history for an individual child.
/// </summary>
public sealed record ChildAttendanceHistoryDto(
    Guid ChildId,
    string? ChildName,
    int TotalDays,
    int PresentDays,
    int LateDays,
    int AbsentDays,
    int EarlyPickupDays,
    double AttendanceRate,
    IReadOnlyList<AttendanceDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
