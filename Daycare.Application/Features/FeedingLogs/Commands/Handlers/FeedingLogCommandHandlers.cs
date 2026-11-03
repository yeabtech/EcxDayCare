using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.FeedingLogs.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.FeedingLogs.Commands.Handlers;

public sealed class FeedingLogCommandHandlers :
    IRequestHandler<CreateFeedingLogCommand, Result<FeedingLogDto>>,
    IRequestHandler<UpdateFeedingLogCommand, Result<FeedingLogDto>>,
    IRequestHandler<DeleteFeedingLogCommand, Result>
{
    private readonly IFeedingLogRepository _feedingLogRepository;
    private readonly IChildRepository _childRepository;
    private readonly IDailyReportRepository _dailyReportRepository;

    public FeedingLogCommandHandlers(
        IFeedingLogRepository feedingLogRepository,
        IChildRepository childRepository,
        IDailyReportRepository dailyReportRepository)
    {
        _feedingLogRepository = feedingLogRepository ?? throw new ArgumentNullException(nameof(feedingLogRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
    }

    public async Task<Result<FeedingLogDto>> Handle(CreateFeedingLogCommand command, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<FeedingLogDto>.Failure("Child not found.");

        var logResult = FeedingLog.Create(
            childId,
            command.Time,
            command.FeedingType,
            command.FoodItems,
            command.Amount,
            command.PortionConsumed,
            command.StaffId,
            command.Notes);

        if (logResult.IsFailure)
            return Result<FeedingLogDto>.Failure(logResult.Error!);

        var log = logResult.Value!;
        var report = await _dailyReportRepository.GetByChildAndDateAsync(childId, command.Time, cancellationToken);
        if (report is not null) log.LinkToDailyReport(report.Id);
        await _feedingLogRepository.AddAsync(log, cancellationToken);

        return Result<FeedingLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result<FeedingLogDto>> Handle(UpdateFeedingLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _feedingLogRepository.GetByIdAsync(FeedingLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result<FeedingLogDto>.Failure("Feeding log not found.");

        var updateResult = log.Update(
            command.Time,
            command.FeedingType,
            command.FoodItems,
            command.Amount,
            command.PortionConsumed,
            command.Notes);

        if (updateResult.IsFailure)
            return Result<FeedingLogDto>.Failure(updateResult.Error!);

        var report = await _dailyReportRepository.GetByChildAndDateAsync(log.ChildId, log.Time, cancellationToken);
        log.LinkToDailyReport(report?.Id);
        await _feedingLogRepository.UpdateAsync(log, cancellationToken);

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<FeedingLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result> Handle(DeleteFeedingLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _feedingLogRepository.GetByIdAsync(FeedingLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result.Failure("Feeding log not found.");

        await _feedingLogRepository.DeleteAsync(log, cancellationToken);
        return Result.Success();
    }

    internal static FeedingLogDto MapToDto(FeedingLog log, Child? child = null)
    {
        var childName = child is not null
            ? $"{child.Name.FirstName} {child.Name.LastName}".Trim()
            : null;

        return new FeedingLogDto(
            Id: log.Id.Value,
            ChildId: log.ChildId.Value,
            ChildName: childName,
            Time: log.Time,
            FeedingType: log.FeedingType,
            FeedingTypeName: log.FeedingType.ToString(),
            FoodItems: log.FoodItems,
            Amount: log.Amount,
            PortionConsumed: log.PortionConsumed,
            PortionConsumedName: log.PortionConsumed.ToString(),
            StaffId: log.StaffId,
            Notes: log.Notes,
            CreatedAt: log.CreatedAt,
            UpdatedAt: log.UpdatedAt);
    }
}
