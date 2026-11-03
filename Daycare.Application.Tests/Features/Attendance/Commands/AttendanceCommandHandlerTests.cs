using Daycare.Application.Features.Attendance.Commands;
using Daycare.Application.Features.Attendance.Commands.Handlers;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace Daycare.Application.Tests.Features.AttendanceTests.Commands;

public class AttendanceCommandHandlerTests
{
    private readonly Mock<IAttendanceRepository> _attendanceRepositoryMock;
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly Mock<IClassroomRepository> _classroomRepositoryMock;
    private readonly AttendanceCommandHandlers _handler;

    private readonly Guid _validChildId = Guid.NewGuid();
    private readonly Guid _validClassroomId = Guid.NewGuid();

    public AttendanceCommandHandlerTests()
    {
        _attendanceRepositoryMock = new Mock<IAttendanceRepository>();
        _childRepositoryMock = new Mock<IChildRepository>();
        _classroomRepositoryMock = new Mock<IClassroomRepository>();

        _handler = new AttendanceCommandHandlers(
            _attendanceRepositoryMock.Object,
            _childRepositoryMock.Object,
            _classroomRepositoryMock.Object);
    }

    private Child CreateTestChild(Guid childId)
    {
        return new Child(
            ChildId.Create(childId),
            new PersonName("Leo", "Vance"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-3)),
            Gender.Male,
            DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task CheckIn_WhenChildDoesNotExist_FailsWithChildNotFound()
    {
        // Arrange
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        var command = new CheckInChildCommand(_validChildId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Child not found");
        _attendanceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckIn_WhenChildAlreadyCheckedInToday_FailsWithDuplicateCheckIn()
    {
        // Arrange
        var child = CreateTestChild(_validChildId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        var pastTime = DateTime.UtcNow.Date.AddDays(-1).AddHours(8);
        var existingAttendance = Attendance.CheckIn(ChildId.Create(_validChildId), pastTime).Value!;

        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttendance);

        var command = new CheckInChildCommand(_validChildId, pastTime.AddHours(1));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already checked in");
        _attendanceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CheckIn_WhenChildAlreadyCheckedOutToday_FailsWithDuplicateCheckIn()
    {
        // Arrange
        var child = CreateTestChild(_validChildId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        var pastTime = DateTime.UtcNow.Date.AddDays(-1).AddHours(8);
        var existingAttendance = Attendance.CheckIn(ChildId.Create(_validChildId), pastTime).Value!;
        existingAttendance.CheckOut(pastTime.AddHours(4));

        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttendance);

        var command = new CheckInChildCommand(_validChildId, pastTime.AddHours(5));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Duplicate check-in is not permitted");
    }

    [Fact]
    public async Task CheckIn_WhenChildAlreadyMarkedAbsentToday_Fails()
    {
        // Arrange
        var child = CreateTestChild(_validChildId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        var pastDate = DateTime.UtcNow.Date.AddDays(-1);
        var existingAttendance = Attendance.MarkAbsent(ChildId.Create(_validChildId), pastDate).Value!;

        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAttendance);

        var command = new CheckInChildCommand(_validChildId, pastDate.AddHours(8));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already recorded as absent");
    }

    [Fact]
    public async Task CheckIn_WithValidData_SucceedsAndPersists()
    {
        // Arrange
        var child = CreateTestChild(_validChildId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);

        var pastTime = DateTime.UtcNow.Date.AddDays(-1).AddHours(8).AddMinutes(30);
        var command = new CheckInChildCommand(
            _validChildId,
            CheckInTime: pastTime,
            DroppedOffBy: "Grandpa",
            Notes: "Healthy and cheerful");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ChildName.Should().Be("Leo Vance");
        result.Value.Status.Should().Be(AttendanceStatus.Present.ToString());
        result.Value.DroppedOffBy.Should().Be("Grandpa");

        _attendanceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CheckOut_WhenNoRecordFound_Fails()
    {
        // Arrange
        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);

        var command = new CheckOutChildCommand(ChildId: _validChildId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Active check-in record not found");
    }

    [Fact]
    public async Task CheckOut_WhenChildNotCheckedIn_Fails()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.Date.AddDays(-1);
        var absentAttendance = Attendance.MarkAbsent(ChildId.Create(_validChildId), pastDate).Value!;

        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(absentAttendance);

        var command = new CheckOutChildCommand(ChildId: _validChildId, CheckOutTime: pastDate.AddHours(14));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Cannot check out a child who has not checked in");
    }

    [Fact]
    public async Task CheckOut_WithValidData_SucceedsAndUpdates()
    {
        // Arrange
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var checkInTime = pastDay.AddHours(8);
        var checkOutTime = pastDay.AddHours(13); // early pickup
        var attendance = Attendance.CheckIn(ChildId.Create(_validChildId), checkInTime).Value!;

        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        var child = CreateTestChild(_validChildId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        var command = new CheckOutChildCommand(
            ChildId: _validChildId,
            CheckOutTime: checkOutTime,
            PickedUpBy: "Mother",
            Notes: "Early appointment");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(AttendanceStatus.EarlyPickup.ToString());
        result.Value.PickedUpBy.Should().Be("Mother");

        _attendanceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAbsent_WhenAlreadyCheckedIn_Fails()
    {
        // Arrange
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var child = CreateTestChild(_validChildId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        var attendance = Attendance.CheckIn(ChildId.Create(_validChildId), pastDay.AddHours(8)).Value!;
        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(attendance);

        var command = new MarkChildAbsentCommand(_validChildId, pastDay);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already checked in");
        _attendanceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAbsent_WithValidData_Succeeds()
    {
        // Arrange
        var pastDay = DateTime.UtcNow.Date.AddDays(-1);
        var child = CreateTestChild(_validChildId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _attendanceRepositoryMock
            .Setup(r => r.GetTodayByChildIdAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Attendance?)null);

        var command = new MarkChildAbsentCommand(_validChildId, pastDay, "Sick today");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Status.Should().Be(AttendanceStatus.Absent.ToString());
        result.Value.Notes.Should().Be("Sick today");

        _attendanceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Attendance>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
