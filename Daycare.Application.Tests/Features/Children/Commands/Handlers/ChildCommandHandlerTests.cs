using Daycare.Application.Features.Children.Commands;
using Daycare.Application.Features.Children.Commands.Handlers;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.ValueObjects;
using Moq;

namespace Daycare.Application.Tests.Features.Children.Commands.Handlers;

public class CreateChildCommandHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly CreateChildCommandHandler _handler;

    public CreateChildCommandHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new CreateChildCommandHandler(_childRepositoryMock.Object, Mock.Of<IParentRepository>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateChildAndReturnSuccess()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2),
            PhotoUrl: "https://example.com/photo.jpg",
            Allergies: "None",
            MedicalNotes: "None");

        _childRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        _childRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidFirstName_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: new string('a', 101),
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CHILD_DATA");
    }

    [Fact]
    public async Task Handle_WithFutureDateOfBirth_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateChildCommand(
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(1),
            Gender: Gender.Male,
            EnrollmentDate: DateTime.UtcNow.AddMonths(-2));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CHILD_DATA");
    }

    [Fact]
    public async Task Handle_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => 
            new CreateChildCommandHandler(null!, Mock.Of<IParentRepository>()));
    }
}

public class UpdateChildCommandHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly UpdateChildCommandHandler _handler;

    public UpdateChildCommandHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new UpdateChildCommandHandler(_childRepositoryMock.Object, Mock.Of<IParentRepository>());
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateChildAndReturnSuccess()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var existingChild = new Child(
            ChildId.New(),
            new PersonName("John", "Doe"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-5)),
            Gender.Male,
            DateTime.UtcNow.AddMonths(-2));

        var command = new UpdateChildCommand(
            ChildId: childId.ToString(),
            FirstName: "Jane",
            LastName: "Smith",
            DateOfBirth: DateTime.UtcNow.AddYears(-6),
            Gender: Gender.Female);

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingChild);

        _childRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _childRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidChildId_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateChildCommand(
            ChildId: "invalid-id",
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Gender.Male);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_ID_FORMAT");
    }

    [Fact]
    public async Task Handle_WithNonExistentChild_ShouldReturnFailure()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var command = new UpdateChildCommand(
            ChildId: childId.ToString(),
            FirstName: "John",
            LastName: "Doe",
            DateOfBirth: DateTime.UtcNow.AddYears(-5),
            Gender: Gender.Male);

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHILD_NOT_FOUND");
    }
}

public class DeleteChildCommandHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly DeleteChildCommandHandler _handler;

    public DeleteChildCommandHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new DeleteChildCommandHandler(_childRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteChildAndReturnSuccess()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var command = new DeleteChildCommand(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _childRepositoryMock
            .Setup(x => x.DeleteAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _childRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidChildId_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteChildCommand("invalid-id");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithNonExistentChild_ShouldReturnFailure()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var command = new DeleteChildCommand(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
    }
}

public class MarkChildInactiveCommandHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly MarkChildInactiveCommandHandler _handler;

    public MarkChildInactiveCommandHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new MarkChildInactiveCommandHandler(_childRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldMarkChildInactiveAndReturnSuccess()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var child = new Child(
            ChildId.New(),
            new PersonName("John", "Doe"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-5)),
            Gender.Male,
            DateTime.UtcNow.AddMonths(-2));

        var command = new MarkChildInactiveCommand(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _childRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _childRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidChildId_ShouldReturnFailure()
    {
        // Arrange
        var command = new MarkChildInactiveCommand("invalid-id");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_ID_FORMAT");
    }

    [Fact]
    public async Task Handle_WithNonExistentChild_ShouldReturnFailure()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var command = new MarkChildInactiveCommand(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHILD_NOT_FOUND");
    }
}

public class MarkChildWithdrawnCommandHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly MarkChildWithdrawnCommandHandler _handler;

    public MarkChildWithdrawnCommandHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new MarkChildWithdrawnCommandHandler(_childRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldMarkChildWithdrawnAndReturnSuccess()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var child = new Child(
            ChildId.New(),
            new PersonName("John", "Doe"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-5)),
            Gender.Male,
            DateTime.UtcNow.AddMonths(-2));

        var command = new MarkChildWithdrawnCommand(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _childRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}

public class ReenrollChildCommandHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly ReenrollChildCommandHandler _handler;

    public ReenrollChildCommandHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new ReenrollChildCommandHandler(_childRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReenrollChildAndReturnSuccess()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var child = new Child(
            ChildId.New(),
            new PersonName("John", "Doe"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-5)),
            Gender.Male,
            DateTime.UtcNow.AddMonths(-2));

        // Mark child as inactive first
        child.MarkInactive();

        var command = new ReenrollChildCommand(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _childRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Child>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithEnrolledChild_ShouldReturnFailure()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var child = new Child(
            ChildId.New(),
            new PersonName("John", "Doe"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-5)),
            Gender.Male,
            DateTime.UtcNow.AddMonths(-2));

        var command = new ReenrollChildCommand(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_CHILD_STATUS");
    }
}
