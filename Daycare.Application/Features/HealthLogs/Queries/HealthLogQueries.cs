using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.HealthLogs.Queries;

public sealed record GetHealthLogByIdQuery(
    Guid Id,
    Guid? RequestingParentId = null) : IRequest<Result<HealthLogDto>>;

public sealed record GetHealthLogsByChildAndDateQuery(
    Guid ChildId,
    DateTime Date,
    Guid? RequestingParentId = null) : IRequest<Result<IReadOnlyList<HealthLogDto>>>;

public sealed record GetHealthLogsByChildQuery(
    Guid ChildId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Guid? RequestingParentId = null) : IRequest<Result<IReadOnlyList<HealthLogDto>>>;

public sealed record GetHealthLogsByEventTypeQuery(
    Guid ChildId,
    int EventType,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Guid? RequestingParentId = null) : IRequest<Result<IReadOnlyList<HealthLogDto>>>;
