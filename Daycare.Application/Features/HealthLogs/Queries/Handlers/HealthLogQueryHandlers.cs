using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.HealthLogs.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.HealthLogs.Queries.Handlers;

/// <summary>
/// Handles all HealthLog queries with strict parent authorization.
/// Parents can only access health logs for their own children.
/// </summary>
public sealed class HealthLogQueryHandlers :
    IRequestHandler<GetHealthLogByIdQuery, Result<HealthLogDto>>,
    IRequestHandler<GetHealthLogsByChildAndDateQuery, Result<IReadOnlyList<HealthLogDto>>>,
    IRequestHandler<GetHealthLogsByChildQuery, Result<IReadOnlyList<HealthLogDto>>>,
    IRequestHandler<GetHealthLogsByEventTypeQuery, Result<IReadOnlyList<HealthLogDto>>>
{
    private readonly IHealthLogRepository _healthLogRepository;
    private readonly IChildRepository _childRepository;
    private readonly IParentRepository _parentRepository;

    public HealthLogQueryHandlers(
        IHealthLogRepository healthLogRepository,
        IChildRepository childRepository,
        IParentRepository parentRepository)
    {
        _healthLogRepository = healthLogRepository ?? throw new ArgumentNullException(nameof(healthLogRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
    }

    public async Task<Result<HealthLogDto>> Handle(GetHealthLogByIdQuery query, CancellationToken cancellationToken)
    {
        var log = await _healthLogRepository.GetByIdAsync(HealthLogId.Create(query.Id), cancellationToken);
        if (log is null)
            return Result<HealthLogDto>.Failure("Health log not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(log.ChildId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<HealthLogDto>.Failure("Unauthorized: You do not have permission to view health logs for this child.");
        }

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<HealthLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result<IReadOnlyList<HealthLogDto>>> Handle(GetHealthLogsByChildAndDateQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<HealthLogDto>>.Failure("Child not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(childId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<IReadOnlyList<HealthLogDto>>.Failure("Unauthorized: You do not have permission to view health logs for this child.");
        }

        var logs = await _healthLogRepository.GetByChildAndDateAsync(childId, query.Date, cancellationToken);

        var dtos = logs
            .Select(l => MapToDto(l, child))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<HealthLogDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<HealthLogDto>>> Handle(GetHealthLogsByChildQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<HealthLogDto>>.Failure("Child not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(childId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<IReadOnlyList<HealthLogDto>>.Failure("Unauthorized: You do not have permission to view health logs for this child.");
        }

        var logs = await _healthLogRepository.GetByChildIdAsync(
            childId,
            query.StartDate,
            query.EndDate,
            cancellationToken);

        var dtos = logs
            .Select(l => MapToDto(l, child))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<HealthLogDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<HealthLogDto>>> Handle(GetHealthLogsByEventTypeQuery query, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(query.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<IReadOnlyList<HealthLogDto>>.Failure("Child not found.");

        if (query.RequestingParentId.HasValue)
        {
            var parents = await _parentRepository.GetParentsByChildAsync(childId, cancellationToken);
            var isParentOfChild = parents.Any(p => p.Id.Value == query.RequestingParentId.Value);
            if (!isParentOfChild)
                return Result<IReadOnlyList<HealthLogDto>>.Failure("Unauthorized: You do not have permission to view health logs for this child.");
        }

        var eventType = (HealthEventType)query.EventType;
        var logs = await _healthLogRepository.GetByChildAndEventTypeAsync(
            childId,
            eventType,
            query.StartDate,
            query.EndDate,
            cancellationToken);

        var dtos = logs
            .Select(l => MapToDto(l, child))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<HealthLogDto>>.Success(dtos);
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static HealthLogDto MapToDto(HealthLog log, Child? child) =>
        new(
            log.Id.Value,
            log.ChildId.Value,
            child?.Name.FullName,
            log.Time,
            log.EventType,
            log.EventType.ToString(),
            log.Description,
            log.StaffId,
            log.Notes,
            log.CreatedAt,
            log.UpdatedAt);
}
