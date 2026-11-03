using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Attendance.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Attendance.Queries.Handlers;

/// <summary>
/// MediatR Handlers executing Attendance Queries.
/// </summary>
public sealed class AttendanceQueryHandlers :
    IRequestHandler<GetTodayAttendanceQuery, Result<DailyAttendanceSummaryDto>>,
    IRequestHandler<GetChildAttendanceHistoryQuery, Result<ChildAttendanceHistoryDto>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IChildRepository _childRepository;

    public AttendanceQueryHandlers(
        IAttendanceRepository attendanceRepository,
        IChildRepository childRepository)
    {
        _attendanceRepository = attendanceRepository ?? throw new ArgumentNullException(nameof(attendanceRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<DailyAttendanceSummaryDto>> Handle(GetTodayAttendanceQuery request, CancellationToken cancellationToken)
    {
        var targetDate = DateTime.SpecifyKind((request.Date ?? DateTime.UtcNow).Date, DateTimeKind.Utc);

        AttendanceStatus? statusFilter = null;
        if (!string.IsNullOrWhiteSpace(request.Status) && Enum.TryParse<AttendanceStatus>(request.Status, true, out var parsedStatus))
        {
            statusFilter = parsedStatus;
        }

        // Get all records for metrics
        var allRecords = (await _attendanceRepository.GetTodayAttendanceAsync(targetDate, request.ClassroomId, null, cancellationToken)).ToList();

        // If status filter requested, filter the items list
        var filteredRecords = statusFilter.HasValue
            ? allRecords.Where(a => a.Status == statusFilter.Value).ToList()
            : allRecords;

        var items = filteredRecords.Select(MapToDto).ToList();

        // Calculate summary metrics across all records today
        int totalPresent = allRecords.Count(a => a.Status == AttendanceStatus.Present);
        int totalLate = allRecords.Count(a => a.Status == AttendanceStatus.Late);
        int totalAbsent = allRecords.Count(a => a.Status == AttendanceStatus.Absent);
        int totalEarlyPickup = allRecords.Count(a => a.Status == AttendanceStatus.EarlyPickup);
        int totalCheckedIn = allRecords.Count(a => a.CheckInTime.HasValue && !a.CheckOutTime.HasValue);

        // Total count of children in system
        int totalEnrolled = await _childRepository.GetCountAsync(cancellationToken);

        var summary = new DailyAttendanceSummaryDto(
            Date: targetDate,
            TotalEnrolled: totalEnrolled,
            TotalPresent: totalPresent,
            TotalLate: totalLate,
            TotalAbsent: totalAbsent,
            TotalEarlyPickup: totalEarlyPickup,
            TotalCurrentlyCheckedIn: totalCheckedIn,
            Items: items);

        return Result<DailyAttendanceSummaryDto>.Success(summary);
    }

    public async Task<Result<ChildAttendanceHistoryDto>> Handle(GetChildAttendanceHistoryQuery request, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(request.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
        {
            return Result<ChildAttendanceHistoryDto>.Failure("Child not found");
        }

        var pagedRecords = (await _attendanceRepository.GetChildAttendanceHistoryAsync(
            childId,
            request.StartDate,
            request.EndDate,
            request.Page,
            request.PageSize,
            cancellationToken)).ToList();

        var totalCount = await _attendanceRepository.GetChildAttendanceCountAsync(
            childId,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        // Calculate cumulative metrics
        int presentDays = pagedRecords.Count(a => a.Status == AttendanceStatus.Present);
        int lateDays = pagedRecords.Count(a => a.Status == AttendanceStatus.Late);
        int absentDays = pagedRecords.Count(a => a.Status == AttendanceStatus.Absent);
        int earlyPickupDays = pagedRecords.Count(a => a.Status == AttendanceStatus.EarlyPickup);
        int totalDays = totalCount > 0 ? totalCount : pagedRecords.Count;

        double attendanceRate = 100.0;
        if (totalDays > 0)
        {
            int attendedDays = presentDays + lateDays + earlyPickupDays;
            attendanceRate = Math.Round((attendedDays * 100.0) / totalDays, 1);
        }

        var childName = $"{child.Name.FirstName} {child.Name.LastName}".Trim();

        var historyDto = new ChildAttendanceHistoryDto(
            ChildId: child.Id.Value,
            ChildName: childName,
            TotalDays: totalDays,
            PresentDays: presentDays,
            LateDays: lateDays,
            AbsentDays: absentDays,
            EarlyPickupDays: earlyPickupDays,
            AttendanceRate: attendanceRate,
            Items: pagedRecords.Select(MapToDto).ToList(),
            Page: request.Page,
            PageSize: request.PageSize,
            TotalCount: totalCount);

        return Result<ChildAttendanceHistoryDto>.Success(historyDto);
    }

    private static AttendanceDto MapToDto(Domain.Entities.Attendance attendance)
    {
        var childName = attendance.Child is not null
            ? $"{attendance.Child.Name.FirstName} {attendance.Child.Name.LastName}".Trim()
            : null;

        return new AttendanceDto(
            Id: attendance.Id.Value,
            ChildId: attendance.ChildId.Value,
            ChildName: childName,
            ClassroomId: attendance.ClassroomId?.Value,
            ClassroomName: attendance.Classroom?.Name,
            Date: attendance.Date,
            CheckInTime: attendance.CheckInTime,
            CheckOutTime: attendance.CheckOutTime,
            Status: attendance.Status.ToString(),
            DroppedOffBy: attendance.DroppedOffBy,
            PickedUpBy: attendance.PickedUpBy,
            Notes: attendance.Notes,
            CreatedAt: attendance.CreatedAt,
            UpdatedAt: attendance.UpdatedAt,
            StaffId: attendance.StaffId,
            CheckOutStaffId: attendance.CheckOutStaffId);
    }
}
