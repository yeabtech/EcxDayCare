using Daycare.Application.Features.Staff.Queries;
using Daycare.Application.Features.Staff.Queries.Handlers;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Moq;

namespace Daycare.Application.Tests.Features.Staff.Queries;

public class StaffQueryHandlerTests
{
    private readonly Mock<IStaffRepository> _staffRepositoryMock;
    private readonly StaffQueryHandlers _handler;

    private readonly DateTime _validHireDate = new(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc);
    private readonly TimeOnly _validShiftStart = new(8, 0);
    private readonly TimeOnly _validShiftEnd = new(17, 0);

    public StaffQueryHandlerTests()
    {
        _staffRepositoryMock = new Mock<IStaffRepository>();
        _handler = new StaffQueryHandlers(_staffRepositoryMock.Object);
    }

    [Fact]
    public async Task GetStaffById_WhenStaffExists_ReturnsSuccess()
    {
        // Arrange
        var staff = Domain.Entities.Staff.Create(
            "Hannah",
            "Abbott",
            StaffRole.Teacher,
            "555-123-4567",
            "hannah@daycare.com",
            _validHireDate,
            _validShiftStart,
            _validShiftEnd).Value!;

        _staffRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(staff);

        var query = new GetStaffByIdQuery(staff.Id.Value);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FirstName.Should().Be("Hannah");
        result.Value.LastName.Should().Be("Abbott");
        result.Value.Email.Should().Be("hannah@daycare.com");
    }

    [Fact]
    public async Task GetStaffById_WhenStaffNotFound_ReturnsFailure()
    {
        _staffRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<StaffId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Staff?)null);

        var query = new GetStaffByIdQuery(Guid.NewGuid());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetAllStaff_WithPagination_ReturnsStaffListResponse()
    {
        // Arrange
        var staffList = new List<Domain.Entities.Staff>
        {
            Domain.Entities.Staff.Create("Alice", "One", StaffRole.Teacher, "555-111-1111", "alice@test.com", _validHireDate, _validShiftStart, _validShiftEnd).Value!,
            Domain.Entities.Staff.Create("Bob", "Two", StaffRole.Aide, "555-222-2222", "bob@test.com", _validHireDate, _validShiftStart, _validShiftEnd).Value!,
        };

        _staffRepositoryMock
            .Setup(x => x.GetAllAsync(1, 10, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(staffList);

        _staffRepositoryMock
            .Setup(x => x.GetTotalCountAsync(null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var query = new GetAllStaffQuery(1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Total.Should().Be(2);
        result.Value.Items.Should().HaveCount(2);
        result.Value.Page.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task SearchStaff_WithSearchTerm_ReturnsMatchingStaff()
    {
        // Arrange
        var matchingStaff = new List<Domain.Entities.Staff>
        {
            Domain.Entities.Staff.Create("Alice", "Searchable", StaffRole.Teacher, "555-111-1111", "alice@test.com", _validHireDate, _validShiftStart, _validShiftEnd).Value!,
        };

        _staffRepositoryMock
            .Setup(x => x.SearchAsync("Searchable", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(matchingStaff);

        _staffRepositoryMock
            .Setup(x => x.GetSearchCountAsync("Searchable", It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var query = new SearchStaffQuery("Searchable", 1, 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Total.Should().Be(1);
        result.Value.Items.First().LastName.Should().Be("Searchable");
    }

    [Fact]
    public async Task GetStaffByClassroom_ReturnsClassroomStaff()
    {
        var classroomId = ClassroomId.Create();
        var classroomStaff = new List<Domain.Entities.Staff>
        {
            Domain.Entities.Staff.Create("Clara", "Teacher", StaffRole.LeadTeacher, "555-999-0000", "clara@test.com", _validHireDate, _validShiftStart, _validShiftEnd, classroomId).Value!,
        };

        _staffRepositoryMock
            .Setup(x => x.GetByClassroomIdAsync(classroomId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(classroomStaff);

        var query = new GetStaffByClassroomQuery(classroomId.Value);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().ClassroomId.Should().Be(classroomId.Value);
    }
}
