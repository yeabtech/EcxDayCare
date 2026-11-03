using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Staff.Commands;

/// <summary>
/// Command to create a new staff member
/// </summary>
public sealed record CreateStaffCommand(
    string FirstName,
    string LastName,
    string Role,
    string Phone,
    string Email,
    DateTime HireDate,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    Guid? ClassroomId = null,
    string? Certifications = null) : IRequest<Result<StaffDto>>;

/// <summary>
/// Command to update an existing staff member
/// </summary>
public sealed record UpdateStaffCommand(
    Guid StaffId,
    string FirstName,
    string LastName,
    string Role,
    string Phone,
    string Email,
    DateTime HireDate,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    Guid? ClassroomId = null,
    string? Certifications = null) : IRequest<Result>;

/// <summary>
/// Command to delete a staff member
/// </summary>
public sealed record DeleteStaffCommand(Guid StaffId) : IRequest<Result>;

/// <summary>
/// Command to assign or unassign a classroom for a staff member
/// </summary>
public sealed record AssignStaffToClassroomCommand(
    Guid StaffId,
    Guid? ClassroomId) : IRequest<Result>;
