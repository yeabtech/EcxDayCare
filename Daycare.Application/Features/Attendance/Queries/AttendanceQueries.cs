using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Attendance.Queries;

/// <summary>
/// Query to retrieve daily daycare attendance summary for a given date (defaults to today).
/// </summary>
public sealed record GetTodayAttendanceQuery(
    DateTime? Date = null,
    Guid? ClassroomId = null,
    string? Status = null) : IRequest<Result<DailyAttendanceSummaryDto>>;

/// <summary>
/// Query to retrieve historical attendance logs and metrics for a specific child.
/// </summary>
public sealed record GetChildAttendanceHistoryQuery(
    Guid ChildId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int Page = 1,
    int PageSize = 50) : IRequest<Result<ChildAttendanceHistoryDto>>;
