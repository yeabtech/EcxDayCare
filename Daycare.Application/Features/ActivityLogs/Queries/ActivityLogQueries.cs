using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.ActivityLogs.Queries;

public sealed record GetActivityLogByIdQuery(Guid Id) : IRequest<Result<ActivityLogDto>>;

public sealed record GetActivityLogsByChildAndDateQuery(
    Guid ChildId,
    DateTime Date) : IRequest<Result<IReadOnlyList<ActivityLogDto>>>;

public sealed record GetActivityLogsByChildQuery(
    Guid ChildId,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<Result<IReadOnlyList<ActivityLogDto>>>;
