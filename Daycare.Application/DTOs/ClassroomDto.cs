namespace Daycare.Application.DTOs;

public sealed record ClassroomDto(
    Guid Id,
    string Name,
    int MinAgeMonths,
    int MaxAgeMonths,
    int MaxCapacity,
    int CurrentEnrollment,
    int AvailableSpots,
    string Status,
    Guid? LeadTeacherId,
    string? Description,
    DateTime CreatedAt,
    DateTime UpdatedAt);
