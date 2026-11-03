namespace Daycare.Application.DTOs;

/// <summary>
/// Data transfer object for Staff member
/// </summary>
public sealed record StaffDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Role,
    Guid? ClassroomId,
    string? ClassroomName,
    string Phone,
    string Email,
    DateTime HireDate,
    string? Certifications,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Request model for creating a staff member
/// </summary>
public sealed record CreateStaffRequest(
    string FirstName,
    string LastName,
    string Role,
    string Phone,
    string Email,
    DateTime HireDate,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    Guid? ClassroomId = null,
    string? Certifications = null);

/// <summary>
/// Request model for updating a staff member
/// </summary>
public sealed record UpdateStaffRequest(
    string FirstName,
    string LastName,
    string Role,
    string Phone,
    string Email,
    DateTime HireDate,
    TimeOnly ShiftStart,
    TimeOnly ShiftEnd,
    Guid? ClassroomId = null,
    string? Certifications = null);

/// <summary>
/// Request model for assigning classroom to staff member
/// </summary>
public sealed record AssignClassroomRequest(Guid? ClassroomId);

/// <summary>
/// Response model for paginated staff list
/// </summary>
public sealed record StaffListResponse(
    IEnumerable<StaffDto> Items,
    int Total,
    int Page,
    int PageSize);
