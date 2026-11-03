using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.ActivityLogs.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.ActivityLogs.Commands.Handlers;

public sealed class ActivityLogCommandHandlers :
    IRequestHandler<CreateActivityLogCommand, Result<ActivityLogDto>>,
    IRequestHandler<UpdateActivityLogCommand, Result<ActivityLogDto>>,
    IRequestHandler<DeleteActivityLogCommand, Result>
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IChildRepository _childRepository;
    private readonly IDailyReportRepository _dailyReportRepository;

    public ActivityLogCommandHandlers(
        IActivityLogRepository activityLogRepository,
        IChildRepository childRepository,
        IDailyReportRepository dailyReportRepository)
    {
        _activityLogRepository = activityLogRepository ?? throw new ArgumentNullException(nameof(activityLogRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
    }

    public async Task<Result<ActivityLogDto>> Handle(CreateActivityLogCommand command, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<ActivityLogDto>.Failure("Child not found.");

        var logResult = ActivityLog.Create(
            childId,
            command.Time,
            command.DurationMinutes,
            command.ActivityType,
            command.Title,
            command.Description,
            command.Participation,
            command.StaffId,
            command.Notes);

        if (logResult.IsFailure)
            return Result<ActivityLogDto>.Failure(logResult.Error!);

        var log = logResult.Value!;
        var report = await _dailyReportRepository.GetByChildAndDateAsync(childId, command.Time, cancellationToken);
        if (report is not null) log.LinkToDailyReport(report.Id);
        await _activityLogRepository.AddAsync(log, cancellationToken);

        return Result<ActivityLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result<ActivityLogDto>> Handle(UpdateActivityLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _activityLogRepository.GetByIdAsync(ActivityLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result<ActivityLogDto>.Failure("Activity log not found.");

        var updateResult = log.Update(
            command.Time,
            command.DurationMinutes,
            command.ActivityType,
            command.Title,
            command.Description,
            command.Participation,
            command.Notes);

        if (updateResult.IsFailure)
            return Result<ActivityLogDto>.Failure(updateResult.Error!);

        var report = await _dailyReportRepository.GetByChildAndDateAsync(log.ChildId, log.Time, cancellationToken);
        log.LinkToDailyReport(report?.Id);
        await _activityLogRepository.UpdateAsync(log, cancellationToken);

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<ActivityLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result> Handle(DeleteActivityLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _activityLogRepository.GetByIdAsync(ActivityLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result.Failure("Activity log not found.");

        await _activityLogRepository.DeleteAsync(log, cancellationToken);
        return Result.Success();
    }

    internal static ActivityLogDto MapToDto(ActivityLog log, Child? child = null)
    {
        var childName = child is not null
            ? $"{child.Name.FirstName} {child.Name.LastName}".Trim()
            : null;

        return new ActivityLogDto(
            Id: log.Id.Value,
            ChildId: log.ChildId.Value,
            ChildName: childName,
            Time: log.Time,
            DurationMinutes: log.DurationMinutes,
            ActivityType: log.ActivityType,
            ActivityTypeName: log.ActivityType.ToString(),
            Title: log.Title,
            Description: log.Description,
            Participation: log.Participation,
            ParticipationName: log.Participation.ToString(),
            StaffId: log.StaffId,
            Notes: log.Notes,
            CreatedAt: log.CreatedAt,
            UpdatedAt: log.UpdatedAt);
    }
}
