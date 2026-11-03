using Daycare.Domain.Entities;
using Xunit;

namespace Daycare.Domain.Tests.Features.DailyReports;

/// <summary>
/// Domain unit tests verifying DailyReport aggregate root invariants and business rules.
/// </summary>
public class DailyReportAggregateTests
{
    private readonly ChildId _validChildId = ChildId.Create(Guid.NewGuid());
    private readonly Guid _validStaffId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var date = DateTime.UtcNow.Date;

        var result = DailyReport.Create(
            _validChildId,
            date,
            _validStaffId,
            mood: "Happy",
            mealsSummary: "Ate all breakfast",
            napSummary: "Slept 1.5 hours",
            activitiesSummary: "Painting and block play",
            diaperSummary: "3 changes, all normal",
            milestones: "Said a new word",
            concerns: null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_validChildId, result.Value.ChildId);
        Assert.Equal(date, result.Value.Date);
        Assert.Equal(_validStaffId, result.Value.StaffId);
        Assert.Equal("Happy", result.Value.Mood);
        Assert.False(result.Value.SentToParent);
        Assert.Null(result.Value.SentAt);
    }

    [Fact]
    public void Create_WithEmptyStaffId_ReturnsFailure()
    {
        var result = DailyReport.Create(
            _validChildId,
            DateTime.UtcNow.Date,
            Guid.Empty,
            mood: "Happy");

        Assert.False(result.IsSuccess);
        Assert.Contains("Staff ID", result.Error);
    }

    [Fact]
    public void Create_WithFutureDate_ReturnsFailure()
    {
        var futureDate = DateTime.UtcNow.Date.AddDays(2);

        var result = DailyReport.Create(
            _validChildId,
            futureDate,
            _validStaffId,
            mood: "Happy");

        Assert.False(result.IsSuccess);
        Assert.Contains("future", result.Error);
    }

    [Fact]
    public void Create_WithDateOlderThan60Days_ReturnsFailure()
    {
        var oldDate = DateTime.UtcNow.Date.AddDays(-61);

        var result = DailyReport.Create(
            _validChildId,
            oldDate,
            _validStaffId,
            mood: "Happy");

        Assert.False(result.IsSuccess);
        Assert.Contains("60 days", result.Error);
    }

    [Fact]
    public void Update_WhenValid_UpdatesFieldsAndTimestamp()
    {
        var report = DailyReport.Create(
            _validChildId,
            DateTime.UtcNow.Date,
            _validStaffId,
            mood: "Calm").Value!;

        var updateResult = report.Update(
            mood: "Excited",
            mealsSummary: "Finished all lunch",
            napSummary: "2 hour nap",
            activitiesSummary: "Outdoor playground",
            diaperSummary: "2 changes",
            milestones: "Climbed the ladder independently",
            concerns: "Minor scrape on knee");

        Assert.True(updateResult.IsSuccess);
        Assert.Equal("Excited", report.Mood);
        Assert.Equal("Finished all lunch", report.MealsSummary);
        Assert.Equal("2 hour nap", report.NapSummary);
        Assert.Equal("Minor scrape on knee", report.Concerns);
    }

    [Fact]
    public void MarkSentToParent_SetsSentToParentAndSentAt()
    {
        var report = DailyReport.Create(
            _validChildId,
            DateTime.UtcNow.Date,
            _validStaffId,
            mood: "Happy").Value!;

        var sendResult = report.MarkSentToParent();

        Assert.True(sendResult.IsSuccess);
        Assert.True(report.SentToParent);
        Assert.NotNull(report.SentAt);
    }

    [Fact]
    public void MarkSentToParent_WhenAlreadySent_ReturnsFailure()
    {
        var report = DailyReport.Create(
            _validChildId,
            DateTime.UtcNow.Date,
            _validStaffId,
            mood: "Happy").Value!;

        report.MarkSentToParent();
        var secondSendResult = report.MarkSentToParent();

        Assert.False(secondSendResult.IsSuccess);
        Assert.Contains("already been sent", secondSendResult.Error);
    }
}
