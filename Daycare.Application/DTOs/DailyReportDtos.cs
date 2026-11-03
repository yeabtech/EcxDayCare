namespace Daycare.Application.DTOs;

/// <summary>
/// Data Transfer Object for a DailyReport.
/// </summary>
public sealed record DailyReportDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime Date,
    string? Mood,
    string? MealsSummary,
    string? NapSummary,
    string? ActivitiesSummary,
    string? DiaperSummary,
    string? Milestones,
    string? Concerns,
    Guid StaffId,
    bool SentToParent,
    DateTime? SentAt,
    DateTime CreatedAt,
    DateTime UpdatedAt);

/// <summary>
/// Request DTO for creating a daily report.
/// </summary>
public sealed record CreateDailyReportRequestDto(
    Guid ChildId,
    DateTime Date,
    string? Mood = null,
    string? MealsSummary = null,
    string? NapSummary = null,
    string? ActivitiesSummary = null,
    string? DiaperSummary = null,
    string? Milestones = null,
    string? Concerns = null);

/// <summary>
/// Request DTO for updating a daily report.
/// </summary>
public sealed record UpdateDailyReportRequestDto(
    string? Mood = null,
    string? MealsSummary = null,
    string? NapSummary = null,
    string? ActivitiesSummary = null,
    string? DiaperSummary = null,
    string? Milestones = null,
    string? Concerns = null);

/// <summary>
/// Paginated list of daily reports for a child.
/// </summary>
public sealed record ChildDailyReportsDto(
    Guid ChildId,
    string? ChildName,
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyList<DailyReportDto> Items);
