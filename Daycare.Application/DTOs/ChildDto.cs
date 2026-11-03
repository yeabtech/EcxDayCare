using Daycare.Domain.Entities;

namespace Daycare.Application.DTOs;

/// <summary>
/// DTO describing which parent is linked to a child and in what role.
/// </summary>
public sealed record ParentAssignmentDto(
    Guid ParentId,
    ParentRole Role);

/// <summary>
/// DTO for creating a new child.
/// </summary>
public sealed record CreateChildDto(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    Gender Gender,
    DateTime EnrollmentDate,
    string? PhotoUrl = null,
    string? Allergies = null,
    string? MedicalNotes = null,
    IReadOnlyList<ParentAssignmentDto>? Parents = null);

/// <summary>
/// DTO for updating an existing child.
/// When <see cref="Parents"/> is provided (even an empty list) the child's ParentChildren
/// rows are replaced; when it is omitted the existing links are left untouched.
/// </summary>
public sealed record UpdateChildDto(
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    Gender Gender,
    string? PhotoUrl = null,
    string? Allergies = null,
    string? MedicalNotes = null,
    IReadOnlyList<ParentAssignmentDto>? Parents = null,
    ChildStatus? Status = null,
    DateTime? EnrollmentDate = null);

/// <summary>
/// DTO for reading a parent link (with role) attached to a child.
/// </summary>
public sealed record ParentLinkDto(
    Guid ParentId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Role);

/// <summary>
/// DTO for reading/displaying child information.
/// </summary>
public sealed record ChildDto(
    string Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    int Age,
    Gender Gender,
    string? PhotoUrl,
    DateTime EnrollmentDate,
    string? Allergies,
    string? MedicalNotes,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// DTO for listing children with pagination.
/// </summary>
public sealed record ChildListDto(
    IReadOnlyList<ChildDto> Children,
    int Total,
    int Skip,
    int Take)
{
    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (Total + Take - 1) / Take;

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int CurrentPage => (Skip / Take) + 1;

    /// <summary>
    /// Indicates whether there are more pages.
    /// </summary>
    public bool HasNextPage => (Skip + Take) < Total;
}
