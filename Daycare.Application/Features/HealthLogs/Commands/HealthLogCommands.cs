using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Entities;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.HealthLogs.Commands;

public sealed record CreateHealthLogCommand(
    Guid ChildId,
    DateTime Time,
    HealthEventType EventType,
    string? Description,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<HealthLogDto>>;

public sealed record UpdateHealthLogCommand(
    Guid Id,
    DateTime Time,
    HealthEventType EventType,
    string? Description,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<HealthLogDto>>;

public sealed record DeleteHealthLogCommand(
    Guid Id,
    Guid StaffId) : IRequest<Result>;
