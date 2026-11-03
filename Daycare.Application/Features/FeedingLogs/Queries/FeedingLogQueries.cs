using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.FeedingLogs.Queries;

public sealed record GetFeedingLogByIdQuery(Guid Id) : IRequest<Result<FeedingLogDto>>;

public sealed record GetFeedingLogsByChildAndDateQuery(
    Guid ChildId,
    DateTime Date) : IRequest<Result<IReadOnlyList<FeedingLogDto>>>;

public sealed record GetFeedingLogsByChildQuery(
    Guid ChildId,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<Result<IReadOnlyList<FeedingLogDto>>>;
