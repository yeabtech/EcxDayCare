using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Classrooms.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Classrooms.Commands.Handlers;

public sealed class ClassroomCommandHandlers :
    IRequestHandler<CreateClassroomCommand, Result<ClassroomDto>>,
    IRequestHandler<UpdateClassroomCommand, Result>,
    IRequestHandler<DeleteClassroomCommand, Result>,
    IRequestHandler<EnrollChildCommand, Result>,
    IRequestHandler<RemoveChildCommand, Result>
{
    private readonly IClassroomRepository _classroomRepository;
    private readonly IChildRepository _childRepository;

    public ClassroomCommandHandlers(
        IClassroomRepository classroomRepository,
        IChildRepository childRepository)
    {
        _classroomRepository = classroomRepository;
        _childRepository = childRepository;
    }

    public async Task<Result<ClassroomDto>> Handle(CreateClassroomCommand command, CancellationToken cancellationToken)
    {
        var createResult = Classroom.Create(
            command.Name,
            command.MinAgeMonths,
            command.MaxAgeMonths,
            command.MaxCapacity,
            command.LeadTeacherId,
            command.Description);

        if (createResult.IsFailure)
            return Result<ClassroomDto>.Failure(createResult.Error!);

        await _classroomRepository.AddAsync(createResult.Value!, cancellationToken);

        return Result<ClassroomDto>.Success(MapToDto(createResult.Value!));
    }

    public async Task<Result> Handle(UpdateClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroomId = ClassroomId.Create(command.ClassroomId);
        var classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
        if (classroom is null)
            return Result.Failure("Classroom not found");

        if (!Enum.TryParse<ClassroomStatus>(command.Status, true, out var status))
            return Result.Failure("Invalid classroom status");

        var updateResult = classroom.Update(
            command.Name,
            command.MinAgeMonths,
            command.MaxAgeMonths,
            command.MaxCapacity,
            status,
            command.LeadTeacherId,
            command.Description);

        if (updateResult.IsFailure)
            return updateResult;

        await _classroomRepository.UpdateAsync(classroom, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(DeleteClassroomCommand command, CancellationToken cancellationToken)
    {
        var classroomId = ClassroomId.Create(command.ClassroomId);
        var classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
        if (classroom is null)
            return Result.Failure("Classroom not found");

        await _classroomRepository.DeleteAsync(classroom, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(EnrollChildCommand command, CancellationToken cancellationToken)
    {
        var classroomId = ClassroomId.Create(command.ClassroomId);
        var classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
        if (classroom is null)
            return Result.Failure("Classroom not found");

        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result.Failure("Child not found");

        var enrollResult = classroom.AddChild(child);
        if (enrollResult.IsFailure)
            return enrollResult;

        await _classroomRepository.UpdateAsync(classroom, cancellationToken);
        await _childRepository.UpdateAsync(child, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(RemoveChildCommand command, CancellationToken cancellationToken)
    {
        var classroomId = ClassroomId.Create(command.ClassroomId);
        var classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
        if (classroom is null)
            return Result.Failure("Classroom not found");

        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
            return Result.Failure("Child not found");

        var removeResult = classroom.RemoveChild(child);
        if (removeResult.IsFailure)
            return removeResult;

        await _classroomRepository.UpdateAsync(classroom, cancellationToken);
        await _childRepository.UpdateAsync(child, cancellationToken);
        return Result.Success();
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
