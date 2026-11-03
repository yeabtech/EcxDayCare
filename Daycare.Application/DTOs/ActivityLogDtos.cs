using System.ComponentModel.DataAnnotations;
using Daycare.Domain.Entities;

namespace Daycare.Application.DTOs;

public sealed record ActivityLogDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime Time,
    int? DurationMinutes,
    ActivityType ActivityType,
    string ActivityTypeName,
    string Title,
    string? Description,
    ParticipationLevel Participation,
    string ParticipationName,
    Guid StaffId,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateActivityLogRequestDto(
    [Required] Guid ChildId,
    [Required] DateTime Time,
    int? DurationMinutes,
    [Required] ActivityType ActivityType,
    [Required, StringLength(200)] string Title,
    string? Description,
    [Required] ParticipationLevel Participation,
    string? Notes);

public sealed record UpdateActivityLogRequestDto(
    [Required] DateTime Time,
    int? DurationMinutes,
    [Required] ActivityType ActivityType,
    [Required, StringLength(200)] string Title,
    string? Description,
    [Required] ParticipationLevel Participation,
    string? Notes);
