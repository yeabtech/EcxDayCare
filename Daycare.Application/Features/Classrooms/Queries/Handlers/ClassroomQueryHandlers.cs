using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Classrooms.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Classrooms.Queries.Handlers;

public sealed class ClassroomQueryHandlers :
    IRequestHandler<GetClassroomQuery, Result<ClassroomDto>>,
    IRequestHandler<GetClassroomChildrenQuery, Result<IReadOnlyList<ChildDto>>>,
    IRequestHandler<GetAllClassroomsQuery, Result<ClassroomListResponse>>,
    IRequestHandler<SearchClassroomsQuery, Result<ClassroomListResponse>>
{
    private readonly IClassroomRepository _classroomRepository;

    public ClassroomQueryHandlers(IClassroomRepository classroomRepository)
    {
        _classroomRepository = classroomRepository;
    }

    public async Task<Result<ClassroomDto>> Handle(GetClassroomQuery query, CancellationToken cancellationToken)
    {
        var classroomId = ClassroomId.Create(query.ClassroomId);
        var classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
        if (classroom is null)
            return Result<ClassroomDto>.Failure("Classroom not found");

        return Result<ClassroomDto>.Success(MapToDto(classroom));
    }

    public async Task<Result<IReadOnlyList<ChildDto>>> Handle(
        GetClassroomChildrenQuery query,
        CancellationToken cancellationToken)
    {
        var classroomId = ClassroomId.Create(query.ClassroomId);
        var classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
        if (classroom is null)
            return Result<IReadOnlyList<ChildDto>>.Failure("Classroom not found");

        IReadOnlyList<ChildDto> children = classroom.Children
            .OrderBy(child => child.Name.FirstName)
            .ThenBy(child => child.Name.LastName)
            .Select(child => new ChildDto(
                child.Id.Value.ToString(),
                child.Name.FirstName,
                child.Name.LastName,
                child.DateOfBirth.Date,
                child.DateOfBirth.Age,
                child.Gender,
                child.PhotoUrl,
                child.EnrollmentDate,
                child.Allergies,
                child.MedicalNotes,
                child.Status.ToString(),
                child.CreatedAt,
                child.UpdatedAt))
            .ToList();

        return Result<IReadOnlyList<ChildDto>>.Success(children);
    }

    public async Task<Result<ClassroomListResponse>> Handle(GetAllClassroomsQuery query, CancellationToken cancellationToken)
    {
        var classrooms = await _classroomRepository.GetAllAsync(query.Page, query.PageSize, cancellationToken);
        var total = await _classroomRepository.GetTotalCountAsync(cancellationToken);

        var dtos = classrooms.Select(MapToDto);
        return Result<ClassroomListResponse>.Success(
            new ClassroomListResponse(dtos, total, query.Page, query.PageSize));
    }

    public async Task<Result<ClassroomListResponse>> Handle(SearchClassroomsQuery query, CancellationToken cancellationToken)
    {
        var classrooms = await _classroomRepository.SearchAsync(query.Term, query.Page, query.PageSize, cancellationToken);
        var allResults = await _classroomRepository.SearchAsync(query.Term, 1, int.MaxValue, cancellationToken);
        var total = allResults.Count();

        var dtos = classrooms.Select(MapToDto);
        return Result<ClassroomListResponse>.Success(
            new ClassroomListResponse(dtos, total, query.Page, query.PageSize));
    }

    private static ClassroomDto MapToDto(Classroom classroom) => new(
        classroom.ClassroomId.Value,
        classroom.Name,
        classroom.AgeGroup.MinAgeMonths,
        classroom.AgeGroup.MaxAgeMonths,
        classroom.Capacity.MaxCapacity,
        classroom.GetCurrentEnrollment(),
        classroom.GetAvailableSpots(),
        classroom.Status.ToString(),
        classroom.LeadTeacherId,
        classroom.Description,
        classroom.CreatedAt,
        classroom.UpdatedAt);
}
