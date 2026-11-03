using Daycare.Application.Features.Staff.Commands;
using Daycare.Application.Features.Staff.Commands.Handlers;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.ValueObjects;
using Moq;

namespace Daycare.Application.Tests.Features.Staff.Commands;

public class StaffCommandHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepositoryMock;
    private readonly Mock<IClassroomRepository> _classroomRepositoryMock;
    private readonly StaffCommandHandlers _handler;

    private readonly DateTime _validHireDate = new(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    private readonly TimeOnly _validShiftStart = new(8, 0);
    private readonly TimeOnly _validShiftEnd = new(17, 0);

    public StaffCommandHandlerTests()
    {
        _staffRepositoryMock = new Mock<IStaffRepository>();
        _classroomRepositoryMock = new Mock<IClassroomRepository>();
        _handler = new StaffCommandHandlers(_staffRepositoryMock.Object, _classroomRepositoryMock.Object);
    }

    [Fact]
    public async Task CreateStaff_WithValidData_ReturnsSuccessAndDto()
    {
        // Arrange
        var command = new CreateStaffCommand(
            "Emily",
            "Clark",
            "Teacher",
            "555-111-2222",
            "emily.clark@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd,
            null,
            "CPR");

        _staffRepositoryMock
            .Setup(x => x.ExistsByEmailAsync("emily.clark@daycare.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _staffRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Domain.Entities.Staff>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be("Emily");
        result.Value.LastName.Should().Be("Clark");
        result.Value.Role.Should().Be("Teacher");
        result.Value.Email.Should().Be("emily.clark@daycare.com");
        _staffRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Staff>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateStaff_WithDuplicateEmail_ReturnsFailure()
    {
        // Arrange
        var command = new CreateStaffCommand(
            "Emily",
            "Clark",
            "Teacher",
            "555-111-2222",
            "emily.clark@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        _staffRepositoryMock
            .Setup(x => x.ExistsByEmailAsync("emily.clark@daycare.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("already exists");
        _staffRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Domain.Entities.Staff>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateStaff_WithNonExistentClassroom_ReturnsFailure()
    {
        var classroomId = Guid.NewGuid();
        var command = new CreateStaffCommand(
            "Emily",
            "Clark",
            "Teacher",
            "555-111-2222",
            "emily.clark@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd,
            classroomId);

        _staffRepositoryMock
            .Setup(x => x.ExistsByEmailAsync("emily.clark@daycare.com", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _classroomRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ClassroomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Classroom?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Classroom not found");
    }

    [Fact]
    public async Task UpdateStaff_WithValidData_UpdatesAndReturnsSuccess()
    {
        // Arrange
        var staffId = Guid.NewGuid();
        var staff = Domain.Entities.Staff.Create(
            "Emily",
            "Clark",
            StaffRole.Teacher,
            "555-111-2222",
            "emily@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        var command = new UpdateStaffCommand(
            staffId,
            "Emily",
            "Davis",
            "LeadTeacher",
            "555-999-8888",
            "emily.davis@daycare.com",
            _validHireDate,
            new TimeOnly(7, 0),
            new TimeOnly(16, 0),
            null,
            "First Aid");

        _staffRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        _staffRepositoryMock
            .Setup(x => x.ExistsByEmailAsync("emily.davis@daycare.com", It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _staffRepositoryMock
            .Setup(x => x.UpdateAsync(staff, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        staff.LastName.Should().Be("Davis");
        staff.Role.Should().Be(StaffRole.LeadTeacher);
        staff.ShiftStart.Should().Be(new TimeOnly(7, 0));
        _staffRepositoryMock.Verify(x => x.UpdateAsync(staff, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateStaff_WhenStaffNotFound_ReturnsFailure()
    {
        var command = new UpdateStaffCommand(
            Guid.NewGuid(),
            "Emily",
            "Clark",
            "Teacher",
            "555-111-2222",
            "emily@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd);

        _staffRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Staff?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task DeleteStaff_WhenStaffExists_DeletesAndReturnsSuccess()
    {
        // Arrange
        var staff = Domain.Entities.Staff.Create(
            "Frank",
            "Miller",
            StaffRole.Aide,
            "555-444-3333",
            "frank@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        _staffRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        _staffRepositoryMock
            .Setup(x => x.DeleteAsync(staff, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteStaffCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _staffRepositoryMock.Verify(x => x.DeleteAsync(staff, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteStaff_WhenStaffNotFound_ReturnsFailure()
    {
        _staffRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Staff?)null);

        var command = new DeleteStaffCommand(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task AssignStaffToClassroom_WithValidClassroom_AssignsAndReturnsSuccess()
    {
        // Arrange
        var staff = Domain.Entities.Staff.Create(
            "Grace",
            "Hopper",
            StaffRole.Teacher,
            "555-777-8888",
            "grace@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        var classroom = Classroom.Create("Infants", 0, 12, 8).Value!;

        _staffRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        _classroomRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ClassroomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(classroom);

        _staffRepositoryMock
            .Setup(x => x.UpdateAsync(staff, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AssignStaffToClassroomCommand(Guid.NewGuid(), classroom.Id.Value);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        staff.ClassroomId.Should().Be(classroom.Id);
        _staffRepositoryMock.Verify(x => x.UpdateAsync(staff, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignStaffToClassroom_WithNullClassroomId_UnassignsStaff()
    {
        // Arrange
        var classroomId = ClassroomId.Create();
        var staff = Domain.Entities.Staff.Create(
            "Grace",
            "Hopper",
            StaffRole.Teacher,
            "555-777-8888",
            "grace@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd,
            classroomId).Value!;

        _staffRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        _staffRepositoryMock
            .Setup(x => x.UpdateAsync(staff, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new AssignStaffToClassroomCommand(Guid.NewGuid(), null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        staff.ClassroomId.Should().BeNull();
        _staffRepositoryMock.Verify(x => x.UpdateAsync(staff, It.IsAny<CancellationToken>()), Times.Once);
    }
}
