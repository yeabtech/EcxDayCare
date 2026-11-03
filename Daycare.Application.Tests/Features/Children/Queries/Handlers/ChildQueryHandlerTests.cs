using Daycare.Application.Features.Children.Queries;
using Daycare.Application.Features.Children.Queries.Handlers;
using Daycare.Application.DTOs;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.ValueObjects;
using Moq;

namespace Daycare.Application.Tests.Features.Children.Queries.Handlers;

public class GetChildByIdQueryHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly GetChildByIdQueryHandler _handler;

    public GetChildByIdQueryHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new GetChildByIdQueryHandler(_childRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidChildId_ShouldReturnChildDto()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var child = new Child(
            new ChildId(childId),
            new PersonName("John", "Doe"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-5)),
            Gender.Male,
            DateTime.UtcNow.AddMonths(-2));

        var query = new GetChildByIdQuery(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Id.Should().Be(childId.ToString());
    }

    [Fact]
    public async Task Handle_WithInvalidChildId_ShouldReturnFailure()
    {
        // Arrange
        var query = new GetChildByIdQuery("invalid-id");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_ID_FORMAT");
    }

    [Fact]
    public async Task Handle_WithNonExistentChild_ShouldReturnFailure()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var query = new GetChildByIdQuery(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CHILD_NOT_FOUND");
    }

    [Fact]
    public async Task Handle_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetChildByIdQueryHandler(null!));
    }
}

public class GetAllChildrenQueryHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly GetAllChildrenQueryHandler _handler;

    public GetAllChildrenQueryHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new GetAllChildrenQueryHandler(_childRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidQuery_ShouldReturnChildListDto()
    {
        // Arrange
        var children = new List<Child>
        {
            new Child(
                new ChildId(Guid.NewGuid()),
                new PersonName("John", "Doe"),
                new DateOfBirth(DateTime.UtcNow.AddYears(-5)),
                Gender.Male,
                DateTime.UtcNow.AddMonths(-2)),
            new Child(
                new ChildId(Guid.NewGuid()),
                new PersonName("Jane", "Smith"),
                new DateOfBirth(DateTime.UtcNow.AddYears(-6)),
                Gender.Female,
                DateTime.UtcNow.AddMonths(-3))
        };

        var query = new GetAllChildrenQuery(0, 10);

        _childRepositoryMock
            .Setup(x => x.GetAllAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(children);

        _childRepositoryMock
            .Setup(x => x.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Children.Should().HaveCount(2);
        result.Value.Total.Should().Be(2);
        result.Value.Skip.Should().Be(0);
        result.Value.Take.Should().Be(10);
    }

    [Fact]
    public async Task Handle_WithEmptyResult_ShouldReturnEmptyChildList()
    {
        // Arrange
        var query = new GetAllChildrenQuery(0, 10);

        _childRepositoryMock
            .Setup(x => x.GetAllAsync(0, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Child>());

        _childRepositoryMock
            .Setup(x => x.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Children.Should().BeEmpty();
        result.Value.Total.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithPagination_ShouldRespectSkipAndTake()
    {
        // Arrange
        var children = new List<Child>
        {
            new Child(
                new ChildId(Guid.NewGuid()),
                new PersonName("John", "Doe"),
                new DateOfBirth(DateTime.UtcNow.AddYears(-5)),
                Gender.Male,
                DateTime.UtcNow.AddMonths(-2))
        };

        var query = new GetAllChildrenQuery(5, 1);

        _childRepositoryMock
            .Setup(x => x.GetAllAsync(5, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(children);

        _childRepositoryMock
            .Setup(x => x.GetCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Skip.Should().Be(5);
        result.Value.Take.Should().Be(1);
        result.Value.Total.Should().Be(10);
        _childRepositoryMock.Verify(x => x.GetAllAsync(5, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new GetAllChildrenQueryHandler(null!));
    }
}

public class ChildExistsQueryHandlerTests
{
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly ChildExistsQueryHandler _handler;

    public ChildExistsQueryHandlerTests()
    {
        _childRepositoryMock = new Mock<IChildRepository>();
        _handler = new ChildExistsQueryHandler(_childRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WithExistingChild_ShouldReturnTrue()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var query = new ChildExistsQuery(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentChild_ShouldReturnFalse()
    {
        // Arrange
        var childId = Guid.NewGuid();
        var query = new ChildExistsQuery(childId.ToString());

        _childRepositoryMock
            .Setup(x => x.ExistsAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithInvalidChildId_ShouldReturnFailure()
    {
        // Arrange
        var query = new ChildExistsQuery("invalid-id");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("INVALID_ID_FORMAT");
    }

    [Fact]
    public async Task Handle_WithNullRepository_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new ChildExistsQueryHandler(null!));
    }
}
