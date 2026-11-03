using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Entities;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.FeedingLogs.Commands;

public sealed record CreateFeedingLogCommand(
    Guid ChildId,
    DateTime Time,
    FeedingType FeedingType,
    string? FoodItems,
    string? Amount,
    PortionConsumed PortionConsumed,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<FeedingLogDto>>;

public sealed record UpdateFeedingLogCommand(
    Guid Id,
    DateTime Time,
    FeedingType FeedingType,
    string? FoodItems,
    string? Amount,
    PortionConsumed PortionConsumed,
    Guid StaffId,
    string? Notes = null) : IRequest<Result<FeedingLogDto>>;

public sealed record DeleteFeedingLogCommand(
    Guid Id,
    Guid StaffId) : IRequest<Result>;
