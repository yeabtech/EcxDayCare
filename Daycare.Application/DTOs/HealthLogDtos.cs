using System.ComponentModel.DataAnnotations;
using Daycare.Domain.Entities;

namespace Daycare.Application.DTOs;

public sealed record HealthLogDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime Time,
    HealthEventType EventType,
    string EventTypeName,
    string? Description,
    Guid StaffId,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateHealthLogRequestDto(
    [Required] Guid ChildId,
    [Required] DateTime Time,
    [EnumDataType(typeof(HealthEventType))] HealthEventType EventType,
    [StringLength(500)] string? Description,
    [StringLength(1000)] string? Notes);

public sealed record UpdateHealthLogRequestDto(
    [Required] DateTime Time,
    [EnumDataType(typeof(HealthEventType))] HealthEventType EventType,
    [StringLength(500)] string? Description,
    [StringLength(1000)] string? Notes);
