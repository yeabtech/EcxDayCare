using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.NapLogs.Commands.Handlers;
using Daycare.Application.Features.NapLogs.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.NapLogs.Queries.Handlers;

public sealed class NapLogQueryHandlers :
    IRequestHandler<GetNapLogByIdQuery, Result<NapLogDto>>,
    IRequestHandler<GetNapLogsByChildAndDateQuery, Result<IReadOnlyList<NapLogDto>>>,
    IRequestHandler<GetNapLogsByChildQuery, Result<IReadOnlyList<NapLogDto>>>
{
    private readonly INapLogRepository _napLogRepository;
    private readonly IChildRepository _childRepository;

    public NapLogQueryHandlers(
        INapLogRepository napLogRepository,
        IChildRepository childRepository)
    {
        _napLogRepository = napLogRepository ?? throw new ArgumentNullException(nameof(napLogRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<NapLogDto>> Handle(GetNapLogByIdQuery query, CancellationToken cancellationToken)
    {
        var log = await _napLogRepository.GetByIdAsync(NapLogId.Create(query.Id), cancellationToken);
        if (log is null)
            return Result<NapLogDto>.Failure("Nap log not found.");

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<NapLogDto>.Success(NapLogCommandHandlers.MapToDto(log, child));
    }

    public async Task<Result<IReadOnlyList<NapLogDto>>> Handle(GetNapLogsByChildAndDateQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<NapLogDto>>.Failure("Child not found.");

        var logs = await _napLogRepository.GetByChildAndDateAsync(childId, query.Date, cancellationToken);
        var dtos = logs.Select(l => NapLogCommandHandlers.MapToDto(l, child)).ToList().AsReadOnly();
        return Result<IReadOnlyList<NapLogDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<NapLogDto>>> Handle(GetNapLogsByChildQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<NapLogDto>>.Failure("Child not found.");

        var logs = await _napLogRepository.GetByChildIdAsync(childId, query.StartDate, query.EndDate, cancellationToken);
        var dtos = logs.Select(l => NapLogCommandHandlers.MapToDto(l, child)).ToList().AsReadOnly();
        return Result<IReadOnlyList<NapLogDto>>.Success(dtos);
    }
}
