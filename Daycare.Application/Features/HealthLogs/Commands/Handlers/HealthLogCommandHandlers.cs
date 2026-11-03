using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.HealthLogs.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.HealthLogs.Commands.Handlers;

public sealed class HealthLogCommandHandlers :
    IRequestHandler<CreateHealthLogCommand, Result<HealthLogDto>>,
    IRequestHandler<UpdateHealthLogCommand, Result<HealthLogDto>>,
    IRequestHandler<DeleteHealthLogCommand, Result>
{
    private readonly IHealthLogRepository _healthLogRepository;
    private readonly IChildRepository _childRepository;

    public HealthLogCommandHandlers(
        IHealthLogRepository healthLogRepository,
        IChildRepository childRepository)
    {
        _healthLogRepository = healthLogRepository ?? throw new ArgumentNullException(nameof(healthLogRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
    }

    public async Task<Result<HealthLogDto>> Handle(CreateHealthLogCommand command, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<HealthLogDto>.Failure("Child not found.");

        var logResult = HealthLog.Create(
            childId,
            command.Time,
            command.EventType,
            command.Description,
            command.StaffId,
            command.Notes);

        if (logResult.IsFailure)
            return Result<HealthLogDto>.Failure(logResult.Error!);

        await _healthLogRepository.AddAsync(logResult.Value!, cancellationToken);

        return Result<HealthLogDto>.Success(MapToDto(logResult.Value!, child));
    }

    public async Task<Result<HealthLogDto>> Handle(UpdateHealthLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _healthLogRepository.GetByIdAsync(HealthLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result<HealthLogDto>.Failure("Health log not found.");

        var updateResult = log.Update(
            command.Time,
            command.EventType,
            command.Description,
            command.Notes);

        if (updateResult.IsFailure)
            return Result<HealthLogDto>.Failure(updateResult.Error!);

        await _healthLogRepository.UpdateAsync(log, cancellationToken);

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<HealthLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result> Handle(DeleteHealthLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _healthLogRepository.GetByIdAsync(HealthLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result.Failure("Health log not found.");

        await _healthLogRepository.DeleteAsync(log, cancellationToken);

        return Result.Success();
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
