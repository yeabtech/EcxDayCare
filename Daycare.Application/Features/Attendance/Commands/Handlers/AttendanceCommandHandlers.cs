using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Attendance.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Attendance.Commands.Handlers;

/// <summary>
/// MediatR Handlers executing Attendance Commands and enforcing domain invariants and business rules.
/// </summary>
public sealed class AttendanceCommandHandlers :
    IRequestHandler<CheckInChildCommand, Result<AttendanceDto>>,
    IRequestHandler<CheckOutChildCommand, Result<AttendanceDto>>,
    IRequestHandler<MarkChildAbsentCommand, Result<AttendanceDto>>
{
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IChildRepository _childRepository;
    private readonly IClassroomRepository _classroomRepository;

    public AttendanceCommandHandlers(
        IAttendanceRepository attendanceRepository,
        IChildRepository childRepository,
        IClassroomRepository classroomRepository)
    {
        _attendanceRepository = attendanceRepository ?? throw new ArgumentNullException(nameof(attendanceRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _classroomRepository = classroomRepository ?? throw new ArgumentNullException(nameof(classroomRepository));
    }

    /// <summary>
    /// Handles child check-in with business rule enforcement against duplicate check-ins.
    /// </summary>
    public async Task<Result<AttendanceDto>> Handle(CheckInChildCommand command, CancellationToken cancellationToken)
    {
        // Business Rule 1: Child must exist
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
        {
            return Result<AttendanceDto>.Failure("Child not found");
        }

        var checkInTime = command.CheckInTime ?? DateTime.UtcNow;
        var date = DateTime.SpecifyKind(checkInTime.Date, DateTimeKind.Utc);

        // Business Rule 2: Prevent invalid duplicate check-ins for the same date
        var existingRecord = await _attendanceRepository.GetTodayByChildIdAsync(childId, date, cancellationToken);
        if (existingRecord is not null)
        {
            if (existingRecord.CheckInTime.HasValue && !existingRecord.CheckOutTime.HasValue)
            {
                return Result<AttendanceDto>.Failure("Child is already checked in for today.");
            }

            if (existingRecord.CheckOutTime.HasValue)
            {
                return Result<AttendanceDto>.Failure("Child has already attended and checked out today. Duplicate check-in is not permitted.");
            }

            if (existingRecord.Status == AttendanceStatus.Absent)
            {
                return Result<AttendanceDto>.Failure("Child is already recorded as absent for today. Cannot create duplicate check-in.");
            }
        }

        // Determine classroom
        ClassroomId? classroomId = null;
        if (command.ClassroomId.HasValue && command.ClassroomId.Value != Guid.Empty)
        {
            classroomId = ClassroomId.Create(command.ClassroomId.Value);
            var classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
            if (classroom is null)
            {
                return Result<AttendanceDto>.Failure("Specified classroom not found");
            }
        }
        else if (child.ClassroomId is not null)
        {
            classroomId = child.ClassroomId;
        }

        // Determine status if explicitly specified
        AttendanceStatus? explicitStatus = null;
        if (!string.IsNullOrWhiteSpace(command.Status) && Enum.TryParse<AttendanceStatus>(command.Status, true, out var parsedStatus))
        {
            explicitStatus = parsedStatus;
        }

        // Domain Factory
        var attendanceResult = Domain.Entities.Attendance.CheckIn(
            childId,
            checkInTime,
            classroomId,
            command.DroppedOffBy,
            command.Notes,
            explicitStatus,
            command.StaffId);

        if (attendanceResult.IsFailure)
        {
            return Result<AttendanceDto>.Failure(attendanceResult.Error!);
        }

        var attendance = attendanceResult.Value!;
        await _attendanceRepository.AddAsync(attendance, cancellationToken);

        return Result<AttendanceDto>.Success(MapToDto(attendance, child));
    }

    /// <summary>
    /// Handles child check-out with validation that child has an active check-in.
    /// </summary>
    public async Task<Result<AttendanceDto>> Handle(CheckOutChildCommand command, CancellationToken cancellationToken)
    {
        Domain.Entities.Attendance? attendance = null;
        var checkOutTime = command.CheckOutTime ?? DateTime.UtcNow;

        if (command.AttendanceId.HasValue && command.AttendanceId.Value != Guid.Empty)
        {
            attendance = await _attendanceRepository.GetByIdAsync(AttendanceId.Create(command.AttendanceId.Value), cancellationToken);
        }
        else if (command.ChildId.HasValue && command.ChildId.Value != Guid.Empty)
        {
            var childId = ChildId.Create(command.ChildId.Value);
            var date = DateTime.SpecifyKind(checkOutTime.Date, DateTimeKind.Utc);
            attendance = await _attendanceRepository.GetTodayByChildIdAsync(childId, date, cancellationToken);
        }

        if (attendance is null)
        {
            return Result<AttendanceDto>.Failure("Active check-in record not found for this child today.");
        }

        // Business Rule: Child must be actively checked in
        if (!attendance.CheckInTime.HasValue || attendance.Status == AttendanceStatus.Absent)
        {
            return Result<AttendanceDto>.Failure("Cannot check out a child who has not checked in or was marked absent.");
        }

        if (attendance.CheckOutTime.HasValue)
        {
            return Result<AttendanceDto>.Failure("Child has already been checked out.");
        }

        if (!command.CheckOutTime.HasValue && attendance.CheckInTime.HasValue && checkOutTime < attendance.CheckInTime.Value)
        {
            checkOutTime = attendance.CheckInTime.Value;
        }

        // Execute Domain CheckOut
        var checkOutResult = attendance.CheckOut(
            checkOutTime,
            command.PickedUpBy,
            command.Notes,
            command.IsEarlyPickup,
            command.StaffId);

        if (checkOutResult.IsFailure)
        {
            return Result<AttendanceDto>.Failure(checkOutResult.Error!);
        }

        await _attendanceRepository.UpdateAsync(attendance, cancellationToken);

        var child = attendance.Child ?? await _childRepository.GetByIdAsync(attendance.ChildId, cancellationToken);
        return Result<AttendanceDto>.Success(MapToDto(attendance, child));
    }

    /// <summary>
    /// Handles marking child absent for a given calendar date.
    /// </summary>
    public async Task<Result<AttendanceDto>> Handle(MarkChildAbsentCommand command, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
        {
            return Result<AttendanceDto>.Failure("Child not found");
        }

        var date = DateTime.SpecifyKind((command.Date ?? DateTime.UtcNow).Date, DateTimeKind.Utc);

        var existingRecord = await _attendanceRepository.GetTodayByChildIdAsync(childId, date, cancellationToken);
        if (existingRecord is not null)
        {
            if (existingRecord.CheckInTime.HasValue)
            {
                return Result<AttendanceDto>.Failure("Child has already checked in today and cannot be marked absent.");
            }

            if (existingRecord.Status == AttendanceStatus.Absent)
            {
                return Result<AttendanceDto>.Failure("Child is already marked absent for this date.");
            }
        }

        var absentResult = Domain.Entities.Attendance.MarkAbsent(
            childId,
            date,
            child.ClassroomId,
            command.Notes,
            command.StaffId);

        if (absentResult.IsFailure)
        {
            return Result<AttendanceDto>.Failure(absentResult.Error!);
        }

        var attendance = absentResult.Value!;
        await _attendanceRepository.AddAsync(attendance, cancellationToken);

        return Result<AttendanceDto>.Success(MapToDto(attendance, child));
    }

    private static AttendanceDto MapToDto(Domain.Entities.Attendance attendance, Child? child)
    {
        var childName = child is not null
            ? $"{child.Name.FirstName} {child.Name.LastName}".Trim()
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
