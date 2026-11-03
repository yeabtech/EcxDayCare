namespace Daycare.Application.DTOs;

/// <summary>
/// Data Transfer Object for Parent
/// </summary>
public sealed record ParentDto(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string Address,
    string Status,
    string? Notes,
    DateTime CreatedAt,
    DateTime UpdatedAt);
