using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Enrollments.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Enrollments.Commands.Handlers;

/// <summary>
/// MediatR Handlers executing Enrollment Commands and enforcing business rules.
/// </summary>
public sealed class EnrollmentCommandHandlers :
    IRequestHandler<CreateEnrollmentCommand, Result<EnrollmentDto>>,
    IRequestHandler<UpdateEnrollmentCommand, Result<EnrollmentDto>>,
    IRequestHandler<EndEnrollmentCommand, Result<EnrollmentDto>>
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IChildRepository _childRepository;
    private readonly IClassroomRepository _classroomRepository;

    public EnrollmentCommandHandlers(
        IEnrollmentRepository enrollmentRepository,
        IChildRepository childRepository,
        IClassroomRepository classroomRepository)
    {
        _enrollmentRepository = enrollmentRepository ?? throw new ArgumentNullException(nameof(enrollmentRepository));
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _classroomRepository = classroomRepository ?? throw new ArgumentNullException(nameof(classroomRepository));
    }

    /// <summary>
    /// Handles enrollment creation with business rule validations.
    /// </summary>
    public async Task<Result<EnrollmentDto>> Handle(CreateEnrollmentCommand command, CancellationToken cancellationToken)
    {
        // Business Rule 1: Child must exist
        var childId = ChildId.Create(command.ChildId);
        var child = await _childRepository.GetByIdAsync(childId, cancellationToken);
        if (child is null)
        {
            return Result<EnrollmentDto>.Failure("Child not found");
        }

        // Business Rule 2: An enrolled child cannot have multiple concurrent active enrollments
        var hasActive = await _enrollmentRepository.HasActiveEnrollmentAsync(childId, null, cancellationToken);
        if (hasActive)
        {
            return Result<EnrollmentDto>.Failure("Child already has an active or pending enrollment. Please end the current enrollment first.");
        }

        // Validate ProgramType
        if (!Enum.TryParse<ProgramType>(command.ProgramType, true, out var programType))
        {
            return Result<EnrollmentDto>.Failure("Invalid program type");
        }

        // Business Rule 3: Classroom capacity should be respected where applicable
        Classroom? classroom = null;
        ClassroomId? classroomId = null;
        if (command.ClassroomId.HasValue)
        {
            classroomId = ClassroomId.Create(command.ClassroomId.Value);
            classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
            if (classroom is null)
            {
                return Result<EnrollmentDto>.Failure("Classroom not found");
            }

            if (classroom.Status != ClassroomStatus.Active)
            {
                return Result<EnrollmentDto>.Failure("Cannot enroll child in an inactive classroom");
            }

            var activeOccupancy = await _enrollmentRepository.GetActiveCountByClassroomIdAsync(classroomId, null, cancellationToken);
            if (classroom.Capacity.IsCapacityFull(activeOccupancy))
            {
                return Result<EnrollmentDto>.Failure($"Classroom '{classroom.Name}' is at full capacity ({classroom.Capacity.MaxCapacity})");
            }
        }

        // Business Rule 4: Dates must be valid & date range cannot be invalid
        var createResult = Enrollment.Create(
            childId,
            programType,
            command.DaysOfWeek,
            command.StartDate,
            command.EndDate,
            command.ContractSigned,
            command.ImmunizationRecord,
            command.BirthCertificate,
            command.MedicalForm,
            classroomId);

        if (createResult.IsFailure)
        {
            return Result<EnrollmentDto>.Failure(createResult.Error!);
        }

        var enrollment = createResult.Value!;

        // Persist enrollment
        await _enrollmentRepository.AddAsync(enrollment, cancellationToken);

        // Update child aggregate status and classroom if assigned
        if (child.Status != ChildStatus.Enrolled)
        {
            try
            {
                child.Reenroll();
            }
            catch
            {
                // Status already enrolled
            }
        }

        if (classroomId != null)
        {
            child.AssignToClassroom(classroomId);
        }
        await _childRepository.UpdateAsync(child, cancellationToken);

        return Result<EnrollmentDto>.Success(MapToDto(enrollment, child, classroom));
    }

    /// <summary>
    /// Handles updating an enrollment.
    /// </summary>
    public async Task<Result<EnrollmentDto>> Handle(UpdateEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollmentId = EnrollmentId.Create(command.EnrollmentId);
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
        {
            return Result<EnrollmentDto>.Failure("Enrollment not found");
        }

        if (!Enum.TryParse<ProgramType>(command.ProgramType, true, out var programType))
        {
            return Result<EnrollmentDto>.Failure("Invalid program type");
        }

        EnrollmentStatus? status = null;
        if (command.Status != null)
        {
            if (!Enum.TryParse<EnrollmentStatus>(command.Status, true, out var parsedStatus) ||
                !Enum.IsDefined(parsedStatus))
            {
                return Result<EnrollmentDto>.Failure("Invalid enrollment status");
            }
            status = parsedStatus;
        }

        if (status is EnrollmentStatus.Active or EnrollmentStatus.Pending &&
            enrollment.Status is EnrollmentStatus.Ended or EnrollmentStatus.Cancelled &&
            await _enrollmentRepository.HasActiveEnrollmentAsync(enrollment.ChildId, enrollmentId, cancellationToken))
        {
            return Result<EnrollmentDto>.Failure("Child already has an active or pending enrollment");
        }

        // Validate classroom capacity if classroom changed
        Classroom? classroom = null;
        ClassroomId? classroomId = null;
        if (command.ClassroomId.HasValue)
        {
            classroomId = ClassroomId.Create(command.ClassroomId.Value);
            classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
            if (classroom is null)
            {
                return Result<EnrollmentDto>.Failure("Classroom not found");
            }

            if (classroom.Status != ClassroomStatus.Active)
            {
                return Result<EnrollmentDto>.Failure("Cannot assign enrollment to an inactive classroom");
            }

            // If switching classrooms or newly assigning, verify capacity
            if (enrollment.ClassroomId != classroomId)
            {
                var activeOccupancy = await _enrollmentRepository.GetActiveCountByClassroomIdAsync(classroomId, enrollmentId, cancellationToken);
                if (classroom.Capacity.IsCapacityFull(activeOccupancy))
                {
                    return Result<EnrollmentDto>.Failure($"Classroom '{classroom.Name}' is at full capacity ({classroom.Capacity.MaxCapacity})");
                }
            }
        }

        var updateResult = enrollment.Update(
            programType,
            command.DaysOfWeek,
            command.StartDate,
            command.EndDate,
            command.ContractSigned,
            command.ImmunizationRecord,
            command.BirthCertificate,
            command.MedicalForm,
            classroomId,
            status,
            command.EndReason);

        if (updateResult.IsFailure)
        {
            return Result<EnrollmentDto>.Failure(updateResult.Error!);
        }

        await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        var child = await _childRepository.GetByIdAsync(enrollment.ChildId, cancellationToken);
        if (child != null)
        {
            var childChanged = false;
            if (enrollment.Status is EnrollmentStatus.Ended or EnrollmentStatus.Cancelled)
            {
                if (!await _enrollmentRepository.HasActiveEnrollmentAsync(enrollment.ChildId, enrollmentId, cancellationToken))
                {
                    if (child.ClassroomId != null)
                    {
                        child.RemoveFromClassroom();
                        childChanged = true;
                    }
                    if (child.Status != ChildStatus.Inactive)
                    {
                        child.MarkInactive();
                        childChanged = true;
                    }
                }
            }
            else
            {
                if (child.Status != ChildStatus.Enrolled)
                {
                    child.Reenroll();
                    childChanged = true;
                }
                if (classroomId != null && child.ClassroomId != classroomId)
                {
                    child.AssignToClassroom(classroomId);
                    childChanged = true;
                }
            }
            if (childChanged)
            {
                await _childRepository.UpdateAsync(child, cancellationToken);
            }
        }

        return Result<EnrollmentDto>.Success(MapToDto(enrollment, child, classroom));
    }

    /// <summary>
    /// Handles ending an enrollment.
    /// </summary>
    public async Task<Result<EnrollmentDto>> Handle(EndEnrollmentCommand command, CancellationToken cancellationToken)
    {
        var enrollmentId = EnrollmentId.Create(command.EnrollmentId);
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId, cancellationToken);
        if (enrollment is null)
        {
            return Result<EnrollmentDto>.Failure("Enrollment not found");
        }

        var endResult = enrollment.EndEnrollment(command.EndDate, command.Reason);
        if (endResult.IsFailure)
        {
            return Result<EnrollmentDto>.Failure(endResult.Error!);
        }

        await _enrollmentRepository.UpdateAsync(enrollment, cancellationToken);

        // Update child if no other active enrollments exist
        var child = await _childRepository.GetByIdAsync(enrollment.ChildId, cancellationToken);
        if (child != null)
        {
            var hasOtherActive = await _enrollmentRepository.HasActiveEnrollmentAsync(enrollment.ChildId, enrollmentId, cancellationToken);
            if (!hasOtherActive)
            {
                child.RemoveFromClassroom();
                try
                {
                    child.MarkInactive();
                }
                catch
                {
                    // Ignore if already inactive
                }
                await _childRepository.UpdateAsync(child, cancellationToken);
            }
        }

        Classroom? classroom = null;
        if (enrollment.ClassroomId != null)
        {
            classroom = await _classroomRepository.GetByIdAsync(enrollment.ClassroomId, cancellationToken);
        }

        return Result<EnrollmentDto>.Success(MapToDto(enrollment, child, classroom));
    }

    private static EnrollmentDto MapToDto(Enrollment enrollment, Child? child, Classroom? classroom)
    {
        var childName = child != null ? $"{child.Name.FirstName} {child.Name.LastName}" : null;
        return new EnrollmentDto(
            Id: enrollment.Id.Value,
            ChildId: enrollment.ChildId.Value,
            ChildName: childName,
            ClassroomId: enrollment.ClassroomId?.Value,
            ClassroomName: classroom?.Name,
            ProgramType: enrollment.ProgramType.ToString(),
            DaysOfWeek: enrollment.DaysOfWeek,
            StartDate: enrollment.StartDate,
            EndDate: enrollment.EndDate,
            ContractSigned: enrollment.ContractSigned,
            ImmunizationRecord: enrollment.ImmunizationRecord,
            BirthCertificate: enrollment.BirthCertificate,
            MedicalForm: enrollment.MedicalForm,
            Status: enrollment.Status.ToString(),
            EndReason: enrollment.EndReason,
            CreatedAt: enrollment.CreatedAt,
            UpdatedAt: enrollment.UpdatedAt);
    }
}
