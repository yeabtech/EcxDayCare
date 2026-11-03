using Daycare.Application.Features.DailyReports.Queries;
using Daycare.Application.Features.DailyReports.Queries.Handlers;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace Daycare.Application.Tests.Features.DailyReports;

public class DailyReportQueryHandlerTests
{
    private readonly Mock<IDailyReportRepository> _reportRepositoryMock;
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly Mock<IParentRepository> _parentRepositoryMock;
    private readonly DailyReportQueryHandlers _handler;

    private readonly Guid _childId = Guid.NewGuid();
    private readonly Guid _staffId = Guid.NewGuid();
    private readonly Guid _parentId = Guid.NewGuid();

    public DailyReportQueryHandlerTests()
    {
        _reportRepositoryMock = new Mock<IDailyReportRepository>();
        _childRepositoryMock = new Mock<IChildRepository>();
        _parentRepositoryMock = new Mock<IParentRepository>();

        _handler = new DailyReportQueryHandlers(
            _reportRepositoryMock.Object,
            _childRepositoryMock.Object,
            _parentRepositoryMock.Object);
    }

    private Child CreateTestChild(Guid childId)
    {
        return new Child(
            ChildId.Create(childId),
            new PersonName("Sophia", "Miller"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-2)),
            Gender.Female,
            DateTime.UtcNow.Date);
    }

    private Parent CreateTestParent(Guid parentId)
    {
        return Parent.Create(
            "David",
            "Miller",
            "david@example.com",
            "+1234567890",
            "123 Main St").Value!;
    }

    [Fact]
    public async Task GetById_WhenReportNotFound_ReturnsFailure()
    {
        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<DailyReportId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyReport?)null);

        var query = new GetDailyReportByIdQuery(Guid.NewGuid());
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task GetById_WhenParentNotLinkedToChild_ReturnsUnauthorized()
    {
        var report = DailyReport.Create(
            ChildId.Create(_childId),
            DateTime.UtcNow.Date,
            _staffId,
            mood: "Happy").Value!;

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<DailyReportId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        // Parent repo returns empty list (this parent is not linked to this child)
        _parentRepositoryMock
            .Setup(r => r.GetParentsByChildAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Parent>());

        var query = new GetDailyReportByIdQuery(report.Id.Value, RequestingParentId: _parentId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task GetChildReports_WhenParentNotLinkedToChild_ReturnsUnauthorized()
    {
        var child = CreateTestChild(_childId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _parentRepositoryMock
            .Setup(r => r.GetParentsByChildAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Parent>());

        var query = new GetChildDailyReportsQuery(_childId, RequestingParentId: _parentId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Unauthorized");
    }

    [Fact]
    public async Task GetChildReports_WhenAuthorizedParent_ReturnsOnlySentReports()
    {
        var child = CreateTestChild(_childId);
        var parent = CreateTestParent(_parentId);

        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _parentRepositoryMock
            .Setup(r => r.GetParentsByChildAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Parent> { parent });

        var sentReport = DailyReport.Create(
            ChildId.Create(_childId),
            DateTime.UtcNow.Date,
            _staffId,
            mood: "Happy").Value!;
        sentReport.MarkSentToParent();

        var draftReport = DailyReport.Create(
            ChildId.Create(_childId),
            DateTime.UtcNow.Date.AddDays(-1),
            _staffId,
            mood: "Calm").Value!;

        _reportRepositoryMock
            .Setup(r => r.GetByChildIdAsync(
                It.IsAny<ChildId>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DailyReport> { sentReport, draftReport });

        _reportRepositoryMock
            .Setup(r => r.GetCountByChildIdAsync(
                It.IsAny<ChildId>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var query = new GetChildDailyReportsQuery(_childId, RequestingParentId: parent.Id.Value);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        // Should only return the sent report
        result.Value!.Items.Should().HaveCount(1);
        result.Value.Items[0].SentToParent.Should().BeTrue();
    }
}
