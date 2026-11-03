using Daycare.Domain.Entities;
using Xunit;

namespace Daycare.Domain.Tests.Features.AttendanceTests;

/// <summary>
/// Domain unit tests verifying Attendance aggregate root invariants,
/// business rules, state transitions, and time validations.
/// </summary>
public class AttendanceAggregateTests
{
    private readonly ChildId _validChildId = ChildId.Create(Guid.NewGuid());
    private readonly ClassroomId _validClassroomId = ClassroomId.Create(Guid.NewGuid());

    [Fact]
    public void CheckIn_WithValidDataOnTime_ReturnsSuccessAndPresentStatus()
    {
        // Check-in on a previous day at 8:30 AM (on-time)
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var checkInTime = pastDay.AddHours(8).AddMinutes(30);

        var result = Attendance.CheckIn(
            _validChildId,
            checkInTime,
            _validClassroomId,
            droppedOffBy: "Sarah Vance (Mother)",
            notes: "Ready for the day");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_validChildId, result.Value.ChildId);
        Assert.Equal(_validClassroomId, result.Value.ClassroomId);
        Assert.Equal(AttendanceStatus.Present, result.Value.Status);
        Assert.Equal(checkInTime, result.Value.CheckInTime);
        Assert.Null(result.Value.CheckOutTime);
        Assert.Equal("Sarah Vance (Mother)", result.Value.DroppedOffBy);
        Assert.Equal("Ready for the day", result.Value.Notes);
    }

    [Fact]
    public void CheckIn_LateArrivalPastMorningCutoff_SetsStatusToLate()
    {
        // 10:15 AM (past 9:30 AM cutoff) on past day
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var checkInTime = pastDay.AddHours(10).AddMinutes(15);

        var result = Attendance.CheckIn(
            _validChildId,
            checkInTime,
            _validClassroomId,
            droppedOffBy: "John Vance (Father)",
            notes: "Traffic on the highway");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(AttendanceStatus.Late, result.Value.Status);
    }

    [Fact]
    public void CheckIn_ExplicitStatusProvided_UsesExplicitStatus()
    {
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var checkInTime = pastDay.AddHours(8).AddMinutes(0);

        var result = Attendance.CheckIn(
            _validChildId,
            checkInTime,
            explicitStatus: AttendanceStatus.Late);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(AttendanceStatus.Late, result.Value.Status);
    }

    [Fact]
    public void CheckIn_FutureTimeExceedingTolerance_ReturnsFailure()
    {
        var futureTime = DateTime.UtcNow.AddHours(2);

        var result = Attendance.CheckIn(_validChildId, futureTime);

        Assert.False(result.IsSuccess);
        Assert.Contains("future", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CheckIn_NullChildId_ReturnsFailure()
    {
        var result = Attendance.CheckIn(null!, DateTime.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Contains("Child ID is required", result.Error);
    }

    [Fact]
    public void MarkAbsent_ValidData_ReturnsSuccessAndAbsentStatus()
    {
        var date = DateTime.UtcNow.Date;

        var result = Attendance.MarkAbsent(
            _validChildId,
            date,
            _validClassroomId,
            notes: "Parent called: mild fever");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(AttendanceStatus.Absent, result.Value.Status);
        Assert.Null(result.Value.CheckInTime);
        Assert.Null(result.Value.CheckOutTime);
        Assert.Equal("Parent called: mild fever", result.Value.Notes);
    }

    [Fact]
    public void CheckOut_WhenNotCheckedIn_ReturnsFailure()
    {
        var date = DateTime.UtcNow.Date;
        var absentRecord = Attendance.MarkAbsent(_validChildId, date).Value!;

        var result = absentRecord.CheckOut(DateTime.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Contains("Cannot check out a child who has not checked in", result.Error);
    }

    [Fact]
    public void CheckOut_WhenAlreadyCheckedOut_ReturnsFailure()
    {
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var checkInTime = pastDay.AddHours(8);
        var attendance = Attendance.CheckIn(_validChildId, checkInTime).Value!;

        var firstCheckOut = checkInTime.AddHours(4);
        var firstResult = attendance.CheckOut(firstCheckOut);
        Assert.True(firstResult.IsSuccess);

        // Second checkout attempt
        var secondResult = attendance.CheckOut(firstCheckOut.AddMinutes(30));

        Assert.False(secondResult.IsSuccess);
        Assert.Contains("already checked out", secondResult.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void CheckOut_EarlierThanCheckIn_ReturnsFailure()
    {
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var checkInTime = pastDay.AddHours(9);
        var attendance = Attendance.CheckIn(_validChildId, checkInTime).Value!;

        var invalidCheckOut = checkInTime.AddMinutes(-30);
        var result = attendance.CheckOut(invalidCheckOut);

        Assert.False(result.IsSuccess);
        Assert.Contains("earlier than check-in time", result.Error);
    }

    [Fact]
    public void CheckOut_BeforeAfternoonCutoff_TransitionsToEarlyPickup()
    {
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        // Checked in at 8:30 AM
        var checkInTime = pastDay.AddHours(8).AddMinutes(30);
        var attendance = Attendance.CheckIn(_validChildId, checkInTime).Value!;
        Assert.Equal(AttendanceStatus.Present, attendance.Status);

        // Checked out at 1:30 PM (before 3:00 PM cutoff)
        var earlyCheckOut = pastDay.AddHours(13).AddMinutes(30);
        var result = attendance.CheckOut(earlyCheckOut, pickedUpBy: "Grandmother", notes: "Dentist appointment");

        Assert.True(result.IsSuccess);
        Assert.Equal(AttendanceStatus.EarlyPickup, attendance.Status);
        Assert.Equal(earlyCheckOut, attendance.CheckOutTime);
        Assert.Equal("Grandmother", attendance.PickedUpBy);
        Assert.Contains("Dentist appointment", attendance.Notes);
    }

    [Fact]
    public void CheckOut_AfterAfternoonCutoff_PreservesPresentStatus()
    {
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        // Checked in at 8:30 AM
        var checkInTime = pastDay.AddHours(8).AddMinutes(30);
        var attendance = Attendance.CheckIn(_validChildId, checkInTime).Value!;

        // Checked out at 5:00 PM (after 3:00 PM cutoff)
        var normalCheckOut = pastDay.AddHours(17).AddMinutes(0);
        var result = attendance.CheckOut(normalCheckOut, isEarlyPickup: false);

        Assert.True(result.IsSuccess);
        Assert.Equal(AttendanceStatus.Present, attendance.Status);
        Assert.Equal(normalCheckOut, attendance.CheckOutTime);
    }

    [Fact]
    public void CheckOut_ExplicitEarlyPickupFlag_OverridesStatusToEarlyPickup()
    {
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var checkInTime = pastDay.AddHours(8).AddMinutes(30);
        var attendance = Attendance.CheckIn(_validChildId, checkInTime).Value!;

        // Even if after cutoff, explicit early pickup flag sets status
        var normalCheckOut = pastDay.AddHours(16).AddMinutes(0);
        var result = attendance.CheckOut(normalCheckOut, isEarlyPickup: true);

        Assert.True(result.IsSuccess);
        Assert.Equal(AttendanceStatus.EarlyPickup, attendance.Status);
    }

    [Fact]
    public void UpdateStatus_UpdatesStatusAndAppendsReason()
    {
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var checkInTime = pastDay.AddHours(8).AddMinutes(30);
        var attendance = Attendance.CheckIn(_validChildId, checkInTime).Value!;

        var result = attendance.UpdateStatus(AttendanceStatus.Late, "Arrived at 9:35 AM officially");

        Assert.True(result.IsSuccess);
        Assert.Equal(AttendanceStatus.Late, attendance.Status);
        Assert.Contains("Arrived at 9:35 AM officially", attendance.Notes);
    }
}
