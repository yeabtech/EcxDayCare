using System.ComponentModel.DataAnnotations;
using Daycare.Domain.Entities;

namespace Daycare.Application.DTOs;

public sealed record NapLogDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime StartTime,
    DateTime? EndTime,
    int? DurationMinutes,
    NapQuality Quality,
    string QualityName,
    Guid StaffId,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateNapLogRequestDto(
    [Required] Guid ChildId,
    [Required] DateTime StartTime,
    DateTime? EndTime,
    [Required] NapQuality Quality,
    string? Notes);

public sealed record UpdateNapLogRequestDto(
    [Required] DateTime StartTime,
    DateTime? EndTime,
    [Required] NapQuality Quality,
    string? Notes);

public sealed record EndNapLogRequestDto(
    [Required] DateTime EndTime,
    NapQuality? Quality,
    string? Notes);
