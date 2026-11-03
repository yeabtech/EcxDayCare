using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Entities;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.ActivityLogs.Commands;

public sealed record CreateActivityLogCommand(
    Guid ChildId,
    DateTime Time,
    int? DurationMinutes,
    ActivityType ActivityType,
    string Title,
    string? Description,
    ParticipationLevel Participation,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<ActivityLogDto>>;

public sealed record UpdateActivityLogCommand(
    Guid Id,
    DateTime Time,
    int? DurationMinutes,
    ActivityType ActivityType,
    string Title,
    string? Description,
    ParticipationLevel Participation,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<ActivityLogDto>>;

public sealed record DeleteActivityLogCommand(
    Guid Id,
    Guid StaffId) : IRequest<Result>;
