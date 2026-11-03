using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.ActivityLogs.Commands.Handlers;
using Daycare.Application.Features.ActivityLogs.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.ActivityLogs.Queries.Handlers;

public sealed class ActivityLogQueryHandlers :
    IRequestHandler<GetActivityLogByIdQuery, Result<ActivityLogDto>>,
    IRequestHandler<GetActivityLogsByChildAndDateQuery, Result<IReadOnlyList<ActivityLogDto>>>,
    IRequestHandler<GetActivityLogsByChildQuery, Result<IReadOnlyList<ActivityLogDto>>>
{
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IChildRepository _childRepository;

    public ActivityLogQueryHandlers(
        IActivityLogRepository activityLogRepository,
        IChildRepository childRepository)
    {
        _activityLogRepository = activityLogRepository ?? throw new ArgumentNullException(nameof(activityLogRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<ActivityLogDto>> Handle(GetActivityLogByIdQuery query, CancellationToken cancellationToken)
    {
        var log = await _activityLogRepository.GetByIdAsync(ActivityLogId.Create(query.Id), cancellationToken);
        if (log is null)
            return Result<ActivityLogDto>.Failure("Activity log not found.");

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<ActivityLogDto>.Success(ActivityLogCommandHandlers.MapToDto(log, child));
    }

    public async Task<Result<IReadOnlyList<ActivityLogDto>>> Handle(GetActivityLogsByChildAndDateQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<ActivityLogDto>>.Failure("Child not found.");

        var logs = await _activityLogRepository.GetByChildAndDateAsync(childId, query.Date, cancellationToken);
        var dtos = logs.Select(l => ActivityLogCommandHandlers.MapToDto(l, child)).ToList().AsReadOnly();
        return Result<IReadOnlyList<ActivityLogDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<ActivityLogDto>>> Handle(GetActivityLogsByChildQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<ActivityLogDto>>.Failure("Child not found.");

        var logs = await _activityLogRepository.GetByChildIdAsync(childId, query.StartDate, query.EndDate, cancellationToken);
        var dtos = logs.Select(l => ActivityLogCommandHandlers.MapToDto(l, child)).ToList().AsReadOnly();
        return Result<IReadOnlyList<ActivityLogDto>>.Success(dtos);
    }
}
