using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.NapLogs.Queries;

public sealed record GetNapLogByIdQuery(Guid Id) : IRequest<Result<NapLogDto>>;

public sealed record GetNapLogsByChildAndDateQuery(
    Guid ChildId,
    DateTime Date) : IRequest<Result<IReadOnlyList<NapLogDto>>>;

public sealed record GetNapLogsByChildQuery(
    Guid ChildId,
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IRequest<Result<IReadOnlyList<NapLogDto>>>;
