using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.DailyReports.Commands;

/// <summary>Command to create a new daily report for a child.</summary>
public sealed record CreateDailyReportCommand(
    Guid ChildId,
    DateTime Date,
    Guid StaffId,
    string? Mood = null,
    string? MealsSummary = null,
    string? NapSummary = null,
    string? ActivitiesSummary = null,
    string? DiaperSummary = null,
    string? Milestones = null,
    string? Concerns = null) : IRequest<Result<DailyReportDto>>;

/// <summary>Command to update an existing daily report.</summary>
public sealed record UpdateDailyReportCommand(
    Guid ReportId,
    Guid StaffId,
    string? Mood = null,
    string? MealsSummary = null,
    string? NapSummary = null,
    string? ActivitiesSummary = null,
    string? DiaperSummary = null,
    string? Milestones = null,
    string? Concerns = null) : IRequest<Result<DailyReportDto>>;

/// <summary>Command to mark a report as sent to the parent.</summary>
public sealed record SendReportToParentCommand(
    Guid ReportId,
    Guid StaffId) : IRequest<Result<DailyReportDto>>;

/// <summary>Command to delete a daily report.</summary>
public sealed record DeleteDailyReportCommand(
    Guid ReportId,
    Guid StaffId) : IRequest<Result>;
