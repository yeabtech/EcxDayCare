using Daycare.Application.Features.Enrollments.Commands;
using Daycare.Application.Features.Enrollments.Commands.Handlers;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace Daycare.Application.Tests.Features.Enrollments.Commands;

public class EnrollmentCommandHandlerTests
{
    private readonly Mock<IEnrollmentRepository> _enrollmentRepositoryMock;
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly Mock<IClassroomRepository> _classroomRepositoryMock;
    private readonly EnrollmentCommandHandlers _handler;

    private readonly Guid _validChildId = Guid.NewGuid();
    private readonly DateTime _validStartDate = DateTime.UtcNow.Date;
    private readonly DateTime _validEndDate = DateTime.UtcNow.Date.AddMonths(6);

    public EnrollmentCommandHandlerTests()
    {
        _enrollmentRepositoryMock = new Mock<IEnrollmentRepository>();
        _childRepositoryMock = new Mock<IChildRepository>();
        _classroomRepositoryMock = new Mock<IClassroomRepository>();

        _handler = new EnrollmentCommandHandlers(
            _enrollmentRepositoryMock.Object,
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

    private Classroom CreateTestClassroom(Guid classroomId, int maxCapacity)
    {
        var ageGroup = AgeGroup.Create(24, 36).Value!;
        var capacity = ClassroomCapacity.Create(maxCapacity).Value!;
        return new Classroom(
            ClassroomId.Create(classroomId),
            "Sunflowers Room",
            ageGroup,
            capacity,
            ClassroomStatus.Active);
    }

    [Fact]
    public async Task CreateEnrollment_WhenChildDoesNotExist_FailsWithChildNotFound()
    {
        // Arrange
        var command = new CreateEnrollmentCommand(
            _validChildId,
            "FullTime",
            "Monday, Wednesday",
            _validStartDate);

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Child not found");
    }

    [Fact]
    public async Task CreateEnrollment_WhenChildAlreadyHasActiveEnrollment_Fails()
    {
        // Arrange
        var child = CreateTestChild(_validChildId);
        var command = new CreateEnrollmentCommand(
            _validChildId,
            "FullTime",
            "Monday, Wednesday",
            _validStartDate);

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _enrollmentRepositoryMock
            .Setup(x => x.HasActiveEnrollmentAsync(It.IsAny<ChildId>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Child already has an active or pending enrollment");
    }

    [Fact]
    public async Task CreateEnrollment_WhenClassroomCapacityExceeded_FailsWithCapacityFull()
    {
        // Arrange
        var child = CreateTestChild(_validChildId);
        var classroomId = Guid.NewGuid();
        var classroom = CreateTestClassroom(classroomId, 10);

        var command = new CreateEnrollmentCommand(
            _validChildId,
            "FullTime",
            "Monday, Tuesday, Wednesday",
            _validStartDate,
            _validEndDate,
            ClassroomId: classroomId);

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _enrollmentRepositoryMock
            .Setup(x => x.HasActiveEnrollmentAsync(It.IsAny<ChildId>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _classroomRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ClassroomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(classroom);

        // Return active count equals capacity (10/10)
        _enrollmentRepositoryMock
            .Setup(x => x.GetActiveCountByClassroomIdAsync(It.IsAny<ClassroomId>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("is at full capacity (10)");
    }

    [Fact]
    public async Task CreateEnrollment_WhenValid_ReturnsSuccessAndDto()
    {
        // Arrange
        var child = CreateTestChild(_validChildId);
        var classroomId = Guid.NewGuid();
        var classroom = CreateTestClassroom(classroomId, 15);

        var command = new CreateEnrollmentCommand(
            _validChildId,
            "FullTime",
            "Monday, Tuesday, Wednesday, Thursday, Friday",
            _validStartDate,
            _validEndDate,
            ContractSigned: true,
            ImmunizationRecord: true,
            BirthCertificate: true,
            MedicalForm: true,
            ClassroomId: classroomId);

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _enrollmentRepositoryMock
            .Setup(x => x.HasActiveEnrollmentAsync(It.IsAny<ChildId>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _classroomRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ClassroomId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(classroom);

        _enrollmentRepositoryMock
            .Setup(x => x.GetActiveCountByClassroomIdAsync(It.IsAny<ClassroomId>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5); // 5/15 spots taken

        _enrollmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _childRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.ChildId.Should().Be(_validChildId);
        result.Value.ProgramType.Should().Be("FullTime");
        result.Value.ClassroomId.Should().Be(classroomId);
        result.Value.ContractSigned.Should().BeTrue();
        result.Value.ImmunizationRecord.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateEnrollment_ChangingStatus_UpdatesChildAndEnrollment()
    {
        var enrollment = Enrollment.Create(ChildId.Create(_validChildId), ProgramType.FullTime,
            "Monday, Wednesday", _validStartDate).Value!;
        var child = CreateTestChild(_validChildId);
        child.Reenroll();
        _enrollmentRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<EnrollmentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);
        _childRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);
        _enrollmentRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _childRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateEnrollmentCommand(enrollment.Id.Value, "FullTime", "Monday, Wednesday",
            _validStartDate, _validEndDate, Status: "Cancelled", EndReason: "Family withdrew");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Cancelled");
        result.Value.EndReason.Should().Be("Family withdrew");
        child.Status.Should().Be(ChildStatus.Inactive);
        _enrollmentRepositoryMock.Verify(x => x.UpdateAsync(enrollment, It.IsAny<CancellationToken>()), Times.Once);
        _childRepositoryMock.Verify(x => x.UpdateAsync(child, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EndEnrollment_WhenValid_SetsStatusEnded()
    {
        // Arrange
        var enrollmentId = Guid.NewGuid();
        var enrollment = Enrollment.Create(
            ChildId.Create(_validChildId),
            ProgramType.FullTime,
            "Monday, Wednesday",
            _validStartDate).Value!;

        var child = CreateTestChild(_validChildId);

        var command = new EndEnrollmentCommand(enrollmentId, _validStartDate.AddMonths(1), "Moved to new area");

        _enrollmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<EnrollmentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(enrollment);

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _enrollmentRepositoryMock
            .Setup(x => x.HasActiveEnrollmentAsync(It.IsAny<ChildId>(), It.IsAny<EnrollmentId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _enrollmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Enrollment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Status.Should().Be("Ended");
        result.Value.EndReason.Should().Be("Moved to new area");
    }
}
