using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Attendance.Commands;

/// <summary>
/// Command to check in a child for today.
/// </summary>
public sealed record CheckInChildCommand(
    Guid ChildId,
    DateTime? CheckInTime = null,
    Guid? ClassroomId = null,
    string? DroppedOffBy = null,
    string? Notes = null,
    string? Status = null,
    Guid? StaffId = null) : IRequest<Result<AttendanceDto>>;

/// <summary>
/// Command to check out a child from daycare.
/// Can identify record either by AttendanceId or ChildId (current active check-in).
/// </summary>
public sealed record CheckOutChildCommand(
    Guid? AttendanceId = null,
    Guid? ChildId = null,
    DateTime? CheckOutTime = null,
    string? PickedUpBy = null,
    string? Notes = null,
    bool? IsEarlyPickup = null,
    Guid? StaffId = null) : IRequest<Result<AttendanceDto>>;

/// <summary>
/// Command to mark a child absent for a specific date.
/// </summary>
public sealed record MarkChildAbsentCommand(
    Guid ChildId,
    DateTime? Date = null,
    string? Notes = null,
    Guid? StaffId = null) : IRequest<Result<AttendanceDto>>;
