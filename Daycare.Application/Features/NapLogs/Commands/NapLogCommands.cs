using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Entities;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.NapLogs.Commands;

public sealed record CreateNapLogCommand(
    Guid ChildId,
    DateTime StartTime,
    DateTime? EndTime,
    NapQuality Quality,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<NapLogDto>>;

public sealed record UpdateNapLogCommand(
    Guid Id,
    DateTime StartTime,
    DateTime? EndTime,
    NapQuality Quality,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<NapLogDto>>;

public sealed record EndNapLogCommand(
    Guid Id,
    DateTime EndTime,
    NapQuality? Quality,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<NapLogDto>>;

public sealed record DeleteNapLogCommand(
    Guid Id,
    Guid StaffId) : IRequest<Result>;
