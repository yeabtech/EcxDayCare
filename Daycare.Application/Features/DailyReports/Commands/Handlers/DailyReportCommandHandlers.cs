using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.DailyReports.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.DailyReports.Commands.Handlers;

/// <summary>
/// Handles all DailyReport commands: Create, Update, SendToParent, Delete.
/// All business rule enforcement lives here — controllers remain thin.
/// </summary>
public sealed class DailyReportCommandHandlers :
    IRequestHandler<CreateDailyReportCommand, Result<DailyReportDto>>,
    IRequestHandler<UpdateDailyReportCommand, Result<DailyReportDto>>,
    IRequestHandler<SendReportToParentCommand, Result<DailyReportDto>>,
    IRequestHandler<DeleteDailyReportCommand, Result>
{
    private readonly IDailyReportRepository _reportRepository;
    private readonly IChildRepository _childRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IFeedingLogRepository _feedingLogRepository;
    private readonly INapLogRepository _napLogRepository;

    public DailyReportCommandHandlers(
        IDailyReportRepository reportRepository,
        IChildRepository childRepository,
        IActivityLogRepository activityLogRepository,
        IFeedingLogRepository feedingLogRepository,
        INapLogRepository napLogRepository)
    {
        _reportRepository = reportRepository ?? throw new ArgumentNullException(nameof(reportRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _activityLogRepository = activityLogRepository ?? throw new ArgumentNullException(nameof(activityLogRepository));
        _feedingLogRepository = feedingLogRepository ?? throw new ArgumentNullException(nameof(feedingLogRepository));
        _napLogRepository = napLogRepository ?? throw new ArgumentNullException(nameof(napLogRepository));
    }

    // ─── Create ───────────────────────────────────────────────────────────────

    public async Task<Result<DailyReportDto>> Handle(CreateDailyReportCommand command, CancellationToken cancellationToken)
    {
        // Business Rule 1: Child must exist
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<DailyReportDto>.Failure("Child not found.");

        // Business Rule 2: Staff ID must be valid
        if (command.StaffId == Guid.Empty)
            return Result<DailyReportDto>.Failure("A valid Staff ID is required to create a report.");

        // Business Rule 3: Only one report per child per day
        var date = DateTime.SpecifyKind(command.Date.Date, DateTimeKind.Utc);
        var alreadyExists = await _reportRepository.ExistsAsync(childId, date, cancellationToken);
        if (alreadyExists)
            return Result<DailyReportDto>.Failure("A daily report already exists for this child on the specified date.");

        // Domain factory — enforces date range rules
        var createResult = DailyReport.Create(
            childId,
            date,
            command.StaffId,
            command.Mood,
            command.MealsSummary,
            command.NapSummary,
            command.ActivitiesSummary,
            command.DiaperSummary,
            command.Milestones,
            command.Concerns);

        if (createResult.IsFailure)
            return Result<DailyReportDto>.Failure(createResult.Error!);

        var report = createResult.Value!;
        await _reportRepository.AddAsync(report, cancellationToken);

        // Logs may be recorded before the report is created. Associate the day's
        // structured entries so the database keeps the report-to-source relationship.
        var activities = await _activityLogRepository.GetByChildAndDateAsync(childId, date, cancellationToken);
        foreach (var log in activities)
        {
            log.LinkToDailyReport(report.Id);
            await _activityLogRepository.UpdateAsync(log, cancellationToken);
        }

        var feedings = await _feedingLogRepository.GetByChildAndDateAsync(childId, date, cancellationToken);
        foreach (var log in feedings)
        {
            log.LinkToDailyReport(report.Id);
            await _feedingLogRepository.UpdateAsync(log, cancellationToken);
        }

        var naps = await _napLogRepository.GetByChildAndDateAsync(childId, date, cancellationToken);
        foreach (var log in naps)
        {
            log.LinkToDailyReport(report.Id);
            await _napLogRepository.UpdateAsync(log, cancellationToken);
        }

        return Result<DailyReportDto>.Success(MapToDto(report, child));
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    public async Task<Result<DailyReportDto>> Handle(UpdateDailyReportCommand command, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(DailyReportId.Create(command.ReportId), cancellationToken);
        if (report is null)
            return Result<DailyReportDto>.Failure("Daily report not found.");

        // Business Rule: Staff must be valid
        if (command.StaffId == Guid.Empty)
            return Result<DailyReportDto>.Failure("A valid Staff ID is required to update a report.");

        var updateResult = report.Update(
            command.Mood,
            command.MealsSummary,
            command.NapSummary,
            command.ActivitiesSummary,
            command.DiaperSummary,
            command.Milestones,
            command.Concerns);

        if (updateResult.IsFailure)
            return Result<DailyReportDto>.Failure(updateResult.Error!);

        await _reportRepository.UpdateAsync(report, cancellationToken);

        var child = report.Child ?? await _childRepository.GetByIdAsync(report.ChildId, cancellationToken);
        return Result<DailyReportDto>.Success(MapToDto(report, child));
    }

    // ─── Send to Parent ───────────────────────────────────────────────────────

    public async Task<Result<DailyReportDto>> Handle(SendReportToParentCommand command, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(DailyReportId.Create(command.ReportId), cancellationToken);
        if (report is null)
            return Result<DailyReportDto>.Failure("Daily report not found.");

        if (command.StaffId == Guid.Empty)
            return Result<DailyReportDto>.Failure("A valid Staff ID is required.");

        var sendResult = report.MarkSentToParent();
        if (sendResult.IsFailure)
            return Result<DailyReportDto>.Failure(sendResult.Error!);

        var child = report.Child ?? await _childRepository.GetByIdAsync(report.ChildId, cancellationToken);
        if (child is null)
            return Result<DailyReportDto>.Failure("Child not found.");

        var parentLinks = await _childRepository.GetParentLinksAsync(report.ChildId, cancellationToken);
        var notifications = parentLinks
            .Select(link => ParentNotification.ForDailyReport(
                link.ParentId,
                report.Id,
                child.Name.FullName,
                report.Date))
            .ToArray();

        await _reportRepository.MarkSentToParentAsync(report, notifications, cancellationToken);

        return Result<DailyReportDto>.Success(MapToDto(report, child));
    }

    // ─── Delete ───────────────────────────────────────────────────────────────

    public async Task<Result> Handle(DeleteDailyReportCommand command, CancellationToken cancellationToken)
    {
        var report = await _reportRepository.GetByIdAsync(DailyReportId.Create(command.ReportId), cancellationToken);
        if (report is null)
            return Result.Failure("Daily report not found.");

        // Business Rule: Cannot delete a report that has already been sent to a parent
        if (report.SentToParent)
            return Result.Failure("Cannot delete a report that has already been sent to the parent.");

        await _reportRepository.DeleteAsync(report, cancellationToken);
        return Result.Success();
    }

    // ─── Mapper ───────────────────────────────────────────────────────────────

    internal static DailyReportDto MapToDto(DailyReport report, Child? child)
    {
        var childName = child is not null
            ? $"{child.Name.FirstName} {child.Name.LastName}".Trim()
            : null;

        return new DailyReportDto(
            Id: report.Id.Value,
            ChildId: report.ChildId.Value,
            ChildName: childName,
            Date: report.Date,
            Mood: report.Mood,
            MealsSummary: report.MealsSummary,
            NapSummary: report.NapSummary,
            ActivitiesSummary: report.ActivitiesSummary,
            DiaperSummary: report.DiaperSummary,
            Milestones: report.Milestones,
            Concerns: report.Concerns,
            StaffId: report.StaffId,
            SentToParent: report.SentToParent,
            SentAt: report.SentAt,
            CreatedAt: report.CreatedAt,
            UpdatedAt: report.UpdatedAt);
    }
}
