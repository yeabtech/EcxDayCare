using Daycare.Application.Features.DailyReports.Commands;
using Daycare.Application.Features.DailyReports.Commands.Handlers;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace Daycare.Application.Tests.Features.DailyReports;

public class DailyReportCommandHandlerTests
{
    private readonly Mock<IDailyReportRepository> _reportRepositoryMock;
    private readonly Mock<IChildRepository> _childRepositoryMock;
    private readonly DailyReportCommandHandlers _handler;

    private readonly Guid _childId = Guid.NewGuid();
    private readonly Guid _staffId = Guid.NewGuid();
    private readonly ParentId _parentId = ParentId.Create();

    public DailyReportCommandHandlerTests()
    {
        _reportRepositoryMock = new Mock<IDailyReportRepository>();
        _childRepositoryMock = new Mock<IChildRepository>();

        _handler = new DailyReportCommandHandlers(
            _reportRepositoryMock.Object,
            _childRepositoryMock.Object,
            Mock.Of<IActivityLogRepository>(),
            Mock.Of<IFeedingLogRepository>(),
            Mock.Of<INapLogRepository>());
    }

    private Child CreateTestChild(Guid childId)
    {
        return new Child(
            ChildId.Create(childId),
            new PersonName("Emma", "Watson"),
            new DateOfBirth(DateTime.UtcNow.AddYears(-3)),
            Gender.Female,
            DateTime.UtcNow.Date);
    }

    [Fact]
    public async Task CreateReport_WhenChildDoesNotExist_ReturnsFailure()
    {
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Child?)null);

        var command = new CreateDailyReportCommand(
            _childId,
            DateTime.UtcNow.Date,
            _staffId,
            Mood: "Happy");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Child not found");
    }

    [Fact]
    public async Task CreateReport_WhenReportAlreadyExistsForDate_ReturnsFailure()
    {
        var child = CreateTestChild(_childId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _reportRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var command = new CreateDailyReportCommand(
            _childId,
            DateTime.UtcNow.Date,
            _staffId,
            Mood: "Happy");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("already exists");
    }

    [Fact]
    public async Task CreateReport_WhenValid_AddsReportAndReturnsDto()
    {
        var child = CreateTestChild(_childId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _reportRepositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<ChildId>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _reportRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<DailyReport>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CreateDailyReportCommand(
            _childId,
            DateTime.UtcNow.Date,
            _staffId,
            Mood: "Excited",
            MealsSummary: "Ate everything",
            NapSummary: "1 hour nap");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ChildId.Should().Be(_childId);
        result.Value.Mood.Should().Be("Excited");
        result.Value.SentToParent.Should().BeFalse();

        _reportRepositoryMock.Verify(r => r.AddAsync(It.IsAny<DailyReport>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendReportToParent_WhenReportNotFound_ReturnsFailure()
    {
        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<DailyReportId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DailyReport?)null);

        var command = new SendReportToParentCommand(Guid.NewGuid(), _staffId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not found");
    }

    [Fact]
    public async Task SendReportToParent_WhenValid_MarksSentAndReturnsDto()
    {
        var report = DailyReport.Create(
            ChildId.Create(_childId),
            DateTime.UtcNow.Date,
            _staffId,
            mood: "Happy",
            milestones: "First steps").Value!;

        _reportRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<DailyReportId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(report);

        var child = CreateTestChild(_childId);
        _childRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(child);

        _childRepositoryMock
            .Setup(r => r.GetParentLinksAsync(It.IsAny<ChildId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new ParentChildLink(_parentId, ParentRole.Guardian) });

        _reportRepositoryMock
            .Setup(r => r.MarkSentToParentAsync(
                It.IsAny<DailyReport>(),
                It.IsAny<IReadOnlyCollection<ParentNotification>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new SendReportToParentCommand(report.Id.Value, _staffId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.SentToParent.Should().BeTrue();
        result.Value.SentAt.Should().NotBeNull();
        result.Value.Milestones.Should().Be("First steps");
        _reportRepositoryMock.Verify(r => r.MarkSentToParentAsync(
            It.IsAny<DailyReport>(),
            It.Is<IReadOnlyCollection<ParentNotification>>(items =>
                items.Count == 1
                && items.Single().ParentId == _parentId
                && items.Single().DailyReportId.Value == report.Id.Value
                && !items.Single().IsRead),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
