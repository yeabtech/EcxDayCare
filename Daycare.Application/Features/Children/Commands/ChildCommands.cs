using MediatR;
using Daycare.Domain.Entities;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Children.Commands;

/// <summary>
/// Command to create a new child.
/// </summary>
public sealed record CreateChildCommand(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    Gender Gender,
    DateTime EnrollmentDate,
    string? PhotoUrl = null,
    string? Allergies = null,
    string? MedicalNotes = null,
    IReadOnlyList<ParentAssignmentDto>? Parents = null) : IRequest<Result<ChildDto>>;

/// <summary>
/// Command to update an existing child.
/// When <see cref="Parents"/> is provided the child's parent links are replaced.
/// </summary>
public sealed record UpdateChildCommand(
    string ChildId,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    Gender Gender,
    string? PhotoUrl = null,
    string? Allergies = null,
    string? MedicalNotes = null,
    IReadOnlyList<ParentAssignmentDto>? Parents = null,
    ChildStatus? Status = null,
    DateTime? EnrollmentDate = null) : IRequest<Result<ChildDto>>;

/// <summary>
/// Command to delete a child.
/// </summary>
public sealed record DeleteChildCommand(string ChildId) : IRequest<Result>;

/// <summary>
/// Command to mark a child as inactive.
/// </summary>
public sealed record MarkChildInactiveCommand(string ChildId) : IRequest<Result<ChildDto>>;

/// <summary>
/// Command to mark a child as withdrawn.
/// </summary>
public sealed record MarkChildWithdrawnCommand(string ChildId) : IRequest<Result<ChildDto>>;

/// <summary>
/// Command to re-enroll an inactive or withdrawn child.
/// </summary>
public sealed record ReenrollChildCommand(string ChildId) : IRequest<Result<ChildDto>>;
