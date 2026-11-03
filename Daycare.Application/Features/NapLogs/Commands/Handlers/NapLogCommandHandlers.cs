using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.NapLogs.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.NapLogs.Commands.Handlers;

public sealed class NapLogCommandHandlers :
    IRequestHandler<CreateNapLogCommand, Result<NapLogDto>>,
    IRequestHandler<UpdateNapLogCommand, Result<NapLogDto>>,
    IRequestHandler<EndNapLogCommand, Result<NapLogDto>>,
    IRequestHandler<DeleteNapLogCommand, Result>
{
    private readonly INapLogRepository _napLogRepository;
    private readonly IChildRepository _childRepository;
    private readonly IDailyReportRepository _dailyReportRepository;

    public NapLogCommandHandlers(
        INapLogRepository napLogRepository,
        IChildRepository childRepository,
        IDailyReportRepository dailyReportRepository)
    {
        _napLogRepository = napLogRepository ?? throw new ArgumentNullException(nameof(napLogRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
    }

    public async Task<Result<NapLogDto>> Handle(CreateNapLogCommand command, CancellationToken cancellationToken)
    {
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result<NapLogDto>.Failure("Child not found.");

        var logResult = NapLog.Create(
            childId,
            command.StartTime,
            command.EndTime,
            command.Quality,
            command.StaffId,
            command.Notes);

        if (logResult.IsFailure)
            return Result<NapLogDto>.Failure(logResult.Error!);

        var log = logResult.Value!;
        var report = await _dailyReportRepository.GetByChildAndDateAsync(childId, command.StartTime, cancellationToken);
        if (report is not null) log.LinkToDailyReport(report.Id);
        await _napLogRepository.AddAsync(log, cancellationToken);

        return Result<NapLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result<NapLogDto>> Handle(UpdateNapLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _napLogRepository.GetByIdAsync(NapLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result<NapLogDto>.Failure("Nap log not found.");

        var updateResult = log.Update(
            command.StartTime,
            command.EndTime,
            command.Quality,
            command.Notes);

        if (updateResult.IsFailure)
            return Result<NapLogDto>.Failure(updateResult.Error!);

        var report = await _dailyReportRepository.GetByChildAndDateAsync(log.ChildId, log.StartTime, cancellationToken);
        log.LinkToDailyReport(report?.Id);
        await _napLogRepository.UpdateAsync(log, cancellationToken);

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<NapLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result<NapLogDto>> Handle(EndNapLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _napLogRepository.GetByIdAsync(NapLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result<NapLogDto>.Failure("Nap log not found.");

        var endResult = log.EndNap(command.EndTime, command.Quality, command.Notes);
        if (endResult.IsFailure)
            return Result<NapLogDto>.Failure(endResult.Error!);

        var report = await _dailyReportRepository.GetByChildAndDateAsync(log.ChildId, log.StartTime, cancellationToken);
        log.LinkToDailyReport(report?.Id);
        await _napLogRepository.UpdateAsync(log, cancellationToken);

        var child = log.Child ?? await _childRepository.GetByIdAsync(log.ChildId, cancellationToken);
        return Result<NapLogDto>.Success(MapToDto(log, child));
    }

    public async Task<Result> Handle(DeleteNapLogCommand command, CancellationToken cancellationToken)
    {
        var log = await _napLogRepository.GetByIdAsync(NapLogId.Create(command.Id), cancellationToken);
        if (log is null)
            return Result.Failure("Nap log not found.");

        await _napLogRepository.DeleteAsync(log, cancellationToken);
        return Result.Success();
    }

    internal static NapLogDto MapToDto(NapLog log, Child? child = null)
    {
        var childName = child is not null
            ? $"{child.Name.FirstName} {child.Name.LastName}".Trim()
            : null;

        return new NapLogDto(
            Id: log.Id.Value,
            ChildId: log.ChildId.Value,
            ChildName: childName,
            StartTime: log.StartTime,
            EndTime: log.EndTime,
            DurationMinutes: log.DurationMinutes,
            Quality: log.Quality,
            QualityName: log.Quality.ToString(),
            StaffId: log.StaffId,
            Notes: log.Notes,
            CreatedAt: log.CreatedAt,
            UpdatedAt: log.UpdatedAt);
    }
}
