using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Staff.Commands;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Staff.Commands.Handlers;

/// <summary>
/// MediatR Command Handlers for Staff CQRS commands
/// </summary>
public sealed class StaffCommandHandlers :
    IRequestHandler<CreateStaffCommand, Result<StaffDto>>,
    IRequestHandler<UpdateStaffCommand, Result>,
    IRequestHandler<DeleteStaffCommand, Result>,
    IRequestHandler<AssignStaffToClassroomCommand, Result>
{
    private readonly IStaffRepository _staffRepository;
    private readonly IClassroomRepository _classroomRepository;

    public StaffCommandHandlers(
        IStaffRepository staffRepository,
        IClassroomRepository classroomRepository)
    {
        _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        _classroomRepository = classroomRepository ?? throw new ArgumentNullException(nameof(classroomRepository));
    }

    public async Task<Result<StaffDto>> Handle(CreateStaffCommand command, CancellationToken cancellationToken)
    {
        // Check duplicate email
        var emailExists = await _staffRepository.ExistsByEmailAsync(command.Email, null, cancellationToken);
        if (emailExists)
            return Result<StaffDto>.Failure("A staff member with this email address already exists");

        // Validate classroom if specified
        Classroom? classroom = null;
        ClassroomId? classroomId = null;
        if (command.ClassroomId.HasValue)
        {
            classroomId = ClassroomId.Create(command.ClassroomId.Value);
            classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
            if (classroom is null)
                return Result<StaffDto>.Failure("Classroom not found");

            if (classroom.Status != ClassroomStatus.Active)
                return Result<StaffDto>.Failure("Cannot assign staff to an inactive classroom");
        }

        // Parse role
        if (!Enum.TryParse<StaffRole>(command.Role, true, out var role))
            return Result<StaffDto>.Failure("Invalid staff role");

        var createResult = Domain.Entities.Staff.Create(
            command.FirstName,
            command.LastName,
            role,
            command.Phone,
            command.Email,
            command.HireDate,
            command.ShiftStart,
            command.ShiftEnd,
            classroomId,
            command.Certifications);

        if (createResult.IsFailure)
            return Result<StaffDto>.Failure(createResult.Error!);

        var staff = createResult.Value!;
        await _staffRepository.AddAsync(staff, cancellationToken);

        // If lead teacher and classroom has no lead teacher, update classroom
        if (role == StaffRole.LeadTeacher && classroom != null && classroom.LeadTeacherId == null)
        {
            classroom.AssignLeadTeacher(staff.Id.Value);
            await _classroomRepository.UpdateAsync(classroom, cancellationToken);
        }

        return Result<StaffDto>.Success(MapToDto(staff, classroom?.Name));
    }

    public async Task<Result> Handle(UpdateStaffCommand command, CancellationToken cancellationToken)
    {
        var staffId = StaffId.Create(command.StaffId);
        var staff = await _staffRepository.GetByIdAsync(staffId, cancellationToken);
        if (staff is null)
            return Result.Failure("Staff member not found");

        // Check duplicate email (excluding self)
        var emailExists = await _staffRepository.ExistsByEmailAsync(command.Email, staffId, cancellationToken);
        if (emailExists)
            return Result.Failure("A staff member with this email address already exists");

        // Validate classroom if specified
        Classroom? classroom = null;
        ClassroomId? classroomId = null;
        if (command.ClassroomId.HasValue)
        {
            classroomId = ClassroomId.Create(command.ClassroomId.Value);
            classroom = await _classroomRepository.GetByIdAsync(classroomId, cancellationToken);
            if (classroom is null)
                return Result.Failure("Classroom not found");

            if (classroom.Status != ClassroomStatus.Active)
                return Result.Failure("Cannot assign staff to an inactive classroom");
        }

        // Parse role
        if (!Enum.TryParse<StaffRole>(command.Role, true, out var role))
            return Result.Failure("Invalid staff role");

        var updateResult = staff.Update(
            command.FirstName,
            command.LastName,
            role,
            command.Phone,
            command.Email,
            command.HireDate,
            command.ShiftStart,
            command.ShiftEnd,
            classroomId,
            command.Certifications);

        if (updateResult.IsFailure)
            return updateResult;

        await _staffRepository.UpdateAsync(staff, cancellationToken);

        // If lead teacher and classroom assigned, update classroom lead teacher if unassigned
        if (role == StaffRole.LeadTeacher && classroom != null && classroom.LeadTeacherId == null)
        {
            classroom.AssignLeadTeacher(staff.Id.Value);
            await _classroomRepository.UpdateAsync(classroom, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> Handle(DeleteStaffCommand command, CancellationToken cancellationToken)
    {
        var staffId = StaffId.Create(command.StaffId);
        var staff = await _staffRepository.GetByIdAsync(staffId, cancellationToken);
        if (staff is null)
            return Result.Failure("Staff member not found");

        // If staff is lead teacher in a classroom, remove lead teacher
        if (staff.ClassroomId is not null)
        {
            var classroom = await _classroomRepository.GetByIdAsync(staff.ClassroomId, cancellationToken);
            if (classroom != null && classroom.LeadTeacherId == staff.Id.Value)
            {
                classroom.AssignLeadTeacher(null);
                await _classroomRepository.UpdateAsync(classroom, cancellationToken);
            }
        }

        await _staffRepository.DeleteAsync(staff, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> Handle(AssignStaffToClassroomCommand command, CancellationToken cancellationToken)
    {
        var staffId = StaffId.Create(command.StaffId);
        var staff = await _staffRepository.GetByIdAsync(staffId, cancellationToken);
        if (staff is null)
            return Result.Failure("Staff member not found");

        if (!command.ClassroomId.HasValue)
        {
            // Unassign from classroom
            if (staff.ClassroomId is not null)
            {
                var oldClassroom = await _classroomRepository.GetByIdAsync(staff.ClassroomId, cancellationToken);
                if (oldClassroom != null && oldClassroom.LeadTeacherId == staff.Id.Value)
                {
                    oldClassroom.AssignLeadTeacher(null);
                    await _classroomRepository.UpdateAsync(oldClassroom, cancellationToken);
                }
            }

            var removeResult = staff.RemoveFromClassroom();
            if (removeResult.IsFailure)
                return removeResult;

            await _staffRepository.UpdateAsync(staff, cancellationToken);
            return Result.Success();
        }

        var newClassroomId = ClassroomId.Create(command.ClassroomId.Value);
        var newClassroom = await _classroomRepository.GetByIdAsync(newClassroomId, cancellationToken);
        if (newClassroom is null)
            return Result.Failure("Classroom not found");

        if (newClassroom.Status != ClassroomStatus.Active)
            return Result.Failure("Cannot assign staff to an inactive classroom");

        // If staff was in another classroom, clear lead teacher if applicable
        if (staff.ClassroomId is not null && staff.ClassroomId != newClassroomId)
        {
            var oldClassroom = await _classroomRepository.GetByIdAsync(staff.ClassroomId, cancellationToken);
            if (oldClassroom != null && oldClassroom.LeadTeacherId == staff.Id.Value)
            {
                oldClassroom.AssignLeadTeacher(null);
                await _classroomRepository.UpdateAsync(oldClassroom, cancellationToken);
            }
        }

        var assignResult = staff.AssignToClassroom(newClassroomId);
        if (assignResult.IsFailure)
            return assignResult;

        if (staff.Role == StaffRole.LeadTeacher && newClassroom.LeadTeacherId == null)
        {
            newClassroom.AssignLeadTeacher(staff.Id.Value);
            await _classroomRepository.UpdateAsync(newClassroom, cancellationToken);
        }

        await _staffRepository.UpdateAsync(staff, cancellationToken);
        return Result.Success();
    }

    private static StaffDto MapToDto(Domain.Entities.Staff staff, string? classroomName = null) => new(
        staff.Id.Value,
        staff.FirstName,
        staff.LastName,
        staff.Role.ToString(),
        staff.ClassroomId?.Value,
        classroomName ?? staff.Classroom?.Name,
        staff.Phone,
        staff.Email,
        staff.HireDate,
        staff.Certifications,
        staff.ShiftStart,
        staff.ShiftEnd,
        staff.CreatedAt,
        staff.UpdatedAt);
}
