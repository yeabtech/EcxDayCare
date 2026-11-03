using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Classrooms.Commands;

public sealed record CreateClassroomCommand(
    string Name,
    int MinAgeMonths,
    int MaxAgeMonths,
    int MaxCapacity,
    Guid? LeadTeacherId = null,
    string? Description = null) : IRequest<Result<ClassroomDto>>;

public sealed record UpdateClassroomCommand(
    Guid ClassroomId,
    string Name,
    int MinAgeMonths,
    int MaxAgeMonths,
    int MaxCapacity,
    string Status,
    Guid? LeadTeacherId = null,
    string? Description = null) : IRequest<Result>;

public sealed record DeleteClassroomCommand(Guid ClassroomId) : IRequest<Result>;

public sealed record EnrollChildCommand(Guid ClassroomId, Guid ChildId) : IRequest<Result>;

public sealed record RemoveChildCommand(Guid ClassroomId, Guid ChildId) : IRequest<Result>;
