using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Classrooms.Queries;

public sealed record GetClassroomQuery(Guid ClassroomId) : IRequest<Result<ClassroomDto>>;

public sealed record GetClassroomChildrenQuery(Guid ClassroomId) : IRequest<Result<IReadOnlyList<ChildDto>>>;

public sealed record GetAllClassroomsQuery(int Page = 1, int PageSize = 10) : IRequest<Result<ClassroomListResponse>>;

public sealed record SearchClassroomsQuery(string Term, int Page = 1, int PageSize = 10) : IRequest<Result<ClassroomListResponse>>;

public sealed record ClassroomListResponse(
    IEnumerable<ClassroomDto> Items,
    int Total,
    int Page,
    int PageSize);
