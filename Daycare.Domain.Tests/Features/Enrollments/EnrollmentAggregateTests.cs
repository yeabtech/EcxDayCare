using Daycare.Domain.Entities;
using Xunit;

namespace Daycare.Domain.Tests.Features.Enrollments;

/// <summary>
/// Unit tests for Enrollment aggregate root and domain invariants.
/// </summary>
public class EnrollmentAggregateTests
{
    private readonly ChildId _childId = ChildId.Create(Guid.NewGuid());
    private readonly DateTime _startDate = DateTime.UtcNow.Date;
    private readonly DateTime _endDate = DateTime.UtcNow.Date.AddMonths(6);

    [Fact]
    public void Create_WithValidData_ReturnsSuccess()
    {
        var result = Enrollment.Create(
            _childId,
            ProgramType.FullTime,
            "Monday, Tuesday, Wednesday, Thursday, Friday",
            _startDate,
            _endDate,
            contractSigned: true,
            immunizationRecord: true,
            birthCertificate: true,
            medicalForm: true);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_childId, result.Value.ChildId);
        Assert.Equal(ProgramType.FullTime, result.Value.ProgramType);
        Assert.Equal("Monday, Tuesday, Wednesday, Thursday, Friday", result.Value.DaysOfWeek);
        Assert.Equal(EnrollmentStatus.Active, result.Value.Status);
        Assert.True(result.Value.ContractSigned);
        Assert.True(result.Value.ImmunizationRecord);
        Assert.True(result.Value.BirthCertificate);
        Assert.True(result.Value.MedicalForm);
    }

    [Fact]
    public void Create_WithEndDateEarlierThanStartDate_ReturnsFailure()
    {
        var invalidEndDate = _startDate.AddDays(-1);

        var result = Enrollment.Create(
            _childId,
            ProgramType.PartTime,
            "Monday, Wednesday",
            _startDate,
            invalidEndDate);

        Assert.False(result.IsSuccess);
        Assert.Contains("End date cannot be earlier than start date", result.Error);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyDaysOfWeek_ReturnsFailure(string? daysOfWeek)
    {
        var result = Enrollment.Create(
            _childId,
            ProgramType.FullTime,
            daysOfWeek!,
            _startDate);

        Assert.False(result.IsSuccess);
        Assert.Contains("Days of week must be specified", result.Error);
    }

    [Fact]
    public void Create_WithFutureStartDate_SetsStatusToPending()
    {
        var futureStart = DateTime.UtcNow.Date.AddDays(14);

        var result = Enrollment.Create(
            _childId,
            ProgramType.Preschool,
            "Monday, Tuesday, Wednesday",
            futureStart);

        Assert.True(result.IsSuccess);
        Assert.Equal(EnrollmentStatus.Pending, result.Value!.Status);
    }

    [Fact]
    public void Update_WithValidData_UpdatesProperties()
    {
        var enrollment = Enrollment.Create(
            _childId,
            ProgramType.FullTime,
            "Monday, Wednesday",
            _startDate).Value!;

        var updateResult = enrollment.Update(
            ProgramType.Toddler,
            "Tuesday, Thursday",
            _startDate,
            _endDate,
            contractSigned: true,
            immunizationRecord: true,
            birthCertificate: false,
            medicalForm: true,
            classroomId: null);

        Assert.True(updateResult.IsSuccess);
        Assert.Equal(ProgramType.Toddler, enrollment.ProgramType);
        Assert.Equal("Tuesday, Thursday", enrollment.DaysOfWeek);
        Assert.True(enrollment.ContractSigned);
    }

    [Fact]
    public void Update_WithInvalidDateRange_ReturnsFailure()
    {
        var enrollment = Enrollment.Create(
            _childId,
            ProgramType.FullTime,
            "Monday, Wednesday",
            _startDate).Value!;

        var invalidEnd = _startDate.AddDays(-5);
        var updateResult = enrollment.Update(
            ProgramType.FullTime,
            "Monday, Wednesday",
            _startDate,
            invalidEnd,
            false, false, false, false, null);

        Assert.False(updateResult.IsSuccess);
        Assert.Contains("End date cannot be earlier than start date", updateResult.Error);
    }

    [Fact]
    public void Update_WithExplicitStatus_PersistsAndCanReopen()
    {
        var enrollment = Enrollment.Create(_childId, ProgramType.FullTime, "Monday, Wednesday", _startDate).Value!;
        var ended = enrollment.Update(ProgramType.FullTime, "Monday, Wednesday", _startDate,
            _startDate.AddDays(30), false, false, false, false, null, EnrollmentStatus.Ended, "Moved away");
        Assert.True(ended.IsSuccess);
        Assert.Equal(EnrollmentStatus.Ended, enrollment.Status);
        Assert.Equal("Moved away", enrollment.EndReason);

        var reopened = enrollment.Update(ProgramType.FullTime, "Monday, Wednesday", _startDate,
            null, false, false, false, false, null, EnrollmentStatus.Active);
        Assert.True(reopened.IsSuccess);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
        Assert.Null(enrollment.EndReason);
    }

    [Fact]
    public void Update_EndedWithoutEndDate_FailsWithoutChangingStatus()
    {
        var enrollment = Enrollment.Create(_childId, ProgramType.FullTime, "Monday, Wednesday", _startDate).Value!;
        var result = enrollment.Update(ProgramType.FullTime, "Monday, Wednesday", _startDate,
            null, false, false, false, false, null, EnrollmentStatus.Ended);
        Assert.False(result.IsSuccess);
        Assert.Equal(EnrollmentStatus.Active, enrollment.Status);
    }

    [Fact]
    public void EndEnrollment_WithValidEndDate_SetsStatusToEnded()
    {
        var enrollment = Enrollment.Create(
            _childId,
            ProgramType.FullTime,
            "Monday, Wednesday",
            _startDate).Value!;

        var endResult = enrollment.EndEnrollment(_startDate.AddDays(30), "Family relocated");

        Assert.True(endResult.IsSuccess);
        Assert.Equal(EnrollmentStatus.Ended, enrollment.Status);
        Assert.Equal("Family relocated", enrollment.EndReason);
    }

    [Fact]
    public void EndEnrollment_WithDateBeforeStart_ReturnsFailure()
    {
        var enrollment = Enrollment.Create(
            _childId,
            ProgramType.FullTime,
            "Monday, Wednesday",
            _startDate).Value!;

        var endResult = enrollment.EndEnrollment(_startDate.AddDays(-1), "Invalid date");

        Assert.False(endResult.IsSuccess);
        Assert.Contains("End date cannot be earlier than start date", endResult.Error);
    }

    [Fact]
    public void EndEnrollment_WhenAlreadyEnded_ReturnsFailure()
    {
        var enrollment = Enrollment.Create(
            _childId,
            ProgramType.FullTime,
            "Monday, Wednesday",
            _startDate).Value!;

        enrollment.EndEnrollment(_startDate.AddDays(10), "Completed");

        var secondEnd = enrollment.EndEnrollment(_startDate.AddDays(20), "Second end");
        Assert.False(secondEnd.IsSuccess);
        Assert.Contains("already ended", secondEnd.Error);
    }

    [Fact]
    public void Cancel_WhenActive_SetsStatusToCancelled()
    {
        var enrollment = Enrollment.Create(
            _childId,
            ProgramType.FullTime,
            "Monday, Wednesday",
            _startDate).Value!;

        var cancelResult = enrollment.Cancel("Parent withdrew before starting");

        Assert.True(cancelResult.IsSuccess);
        Assert.Equal(EnrollmentStatus.Cancelled, enrollment.Status);
        Assert.Equal("Parent withdrew before starting", enrollment.EndReason);
    }
}
