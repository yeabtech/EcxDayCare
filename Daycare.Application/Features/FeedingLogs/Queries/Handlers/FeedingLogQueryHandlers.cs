using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.FeedingLogs.Commands.Handlers;
using Daycare.Application.Features.FeedingLogs.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.FeedingLogs.Queries.Handlers;

public sealed class FeedingLogQueryHandlers :
    IRequestHandler<GetFeedingLogByIdQuery, Result<FeedingLogDto>>,
    IRequestHandler<GetFeedingLogsByChildAndDateQuery, Result<IReadOnlyList<FeedingLogDto>>>,
    IRequestHandler<GetFeedingLogsByChildQuery, Result<IReadOnlyList<FeedingLogDto>>>
{
    private readonly IFeedingLogRepository _feedingLogRepository;
    private readonly IChildRepository _childRepository;

    public FeedingLogQueryHandlers(
        IFeedingLogRepository feedingLogRepository,
        IChildRepository childRepository)
    {
        _feedingLogRepository = feedingLogRepository ?? throw new ArgumentNullException(nameof(feedingLogRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<FeedingLogDto>> Handle(GetFeedingLogByIdQuery query, CancellationToken cancellationToken)
    {
        var log = await _feedingLogRepository.GetByIdAsync(FeedingLogId.Create(query.Id), cancellationToken);
        if (log is null)
            return Result<FeedingLogDto>.Failure("Feeding log not found.");

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<FeedingLogDto>.Success(FeedingLogCommandHandlers.MapToDto(log, child));
    }

    public async Task<Result<IReadOnlyList<FeedingLogDto>>> Handle(GetFeedingLogsByChildAndDateQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<FeedingLogDto>>.Failure("Child not found.");

        var logs = await _feedingLogRepository.GetByChildAndDateAsync(childId, query.Date, cancellationToken);
        var dtos = logs.Select(l => FeedingLogCommandHandlers.MapToDto(l, child)).ToList().AsReadOnly();
        return Result<IReadOnlyList<FeedingLogDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<FeedingLogDto>>> Handle(GetFeedingLogsByChildQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<FeedingLogDto>>.Failure("Child not found.");

        var logs = await _feedingLogRepository.GetByChildIdAsync(childId, query.StartDate, query.EndDate, cancellationToken);
        var dtos = logs.Select(l => FeedingLogCommandHandlers.MapToDto(l, child)).ToList().AsReadOnly();
        return Result<IReadOnlyList<FeedingLogDto>>.Success(dtos);
    }
}
