using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Incidents.Queries;

public sealed record GetIncidentByIdQuery(
    Guid Id,
    Guid? RequestingParentId = null) : IRequest<Result<IncidentDto>>;

public sealed record GetIncidentsByChildQuery(
    Guid ChildId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    Guid? RequestingParentId = null) : IRequest<Result<IReadOnlyList<IncidentDto>>>;

public sealed record GetIncidentsByDateQuery(
    DateTime Date) : IRequest<Result<IReadOnlyList<IncidentDto>>>;

public sealed record GetUnsignedIncidentsByChildQuery(
    Guid ChildId,
    Guid? RequestingParentId = null) : IRequest<Result<IReadOnlyList<IncidentDto>>>;
