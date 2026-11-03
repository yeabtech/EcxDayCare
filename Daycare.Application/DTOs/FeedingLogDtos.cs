using System.ComponentModel.DataAnnotations;
using Daycare.Domain.Entities;

namespace Daycare.Application.DTOs;

public sealed record FeedingLogDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime Time,
    FeedingType FeedingType,
    string FeedingTypeName,
    string? FoodItems,
    string? Amount,
    PortionConsumed PortionConsumed,
    string PortionConsumedName,
    Guid StaffId,
    string? Notes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record CreateFeedingLogRequestDto(
    [Required] Guid ChildId,
    [Required] DateTime Time,
    [Required] FeedingType FeedingType,
    string? FoodItems,
    string? Amount,
    [Required] PortionConsumed PortionConsumed,
    string? Notes);

public sealed record UpdateFeedingLogRequestDto(
    [Required] DateTime Time,
    [Required] FeedingType FeedingType,
    string? FoodItems,
    string? Amount,
    [Required] PortionConsumed PortionConsumed,
    string? Notes);
