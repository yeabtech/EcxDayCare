using Daycare.Domain.Entities;

namespace Daycare.Application.DTOs;

// ─── Admin Dashboard ───────────────────────────────────────────────────────────

public sealed record AdminDashboardDto(
    // Summary Cards
    int TotalChildren,
    int TodaysPresentCount,
    int TodaysAbsentCount,
    int TotalClassrooms,
    int TotalStaff,

    // Recent Activities
    IReadOnlyList<IncidentSummaryDto> RecentIncidents,
    IReadOnlyList<EnrollmentSummaryDto> RecentEnrollments,

    // Breakdown
    IReadOnlyList<ClassroomChildrenBreakdownDto> ChildrenByClassroom);

public sealed record IncidentSummaryDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime IncidentTime,
    string Location,
    string Description,
    InjuryType InjuryType,
    Guid StaffId);

public sealed record EnrollmentSummaryDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime EnrollmentDate,
    EnrollmentStatus Status);

public sealed record ClassroomChildrenBreakdownDto(
    Guid ClassroomId,
    string ClassroomName,
    int ChildCount,
    int StaffCount);

// ─── Staff Dashboard ───────────────────────────────────────────────────────────

public sealed record StaffDashboardDto(
    Guid StaffId,
    string StaffName,

    // Classroom Info
    Guid? AssignedClassroomId,
    string? AssignedClassroomName,
    int ChildrenInClassroom,

    // Today's Status
    int TodaysPresentCount,
    int TodaysAbsentCount,
    int PendingDailyReportsCount,

    // Recent Activities
    IReadOnlyList<DailyActivitySummaryDto> TodaysSchedule,
    IReadOnlyList<IncidentSummaryDto> RecentIncidents,
    IReadOnlyList<HealthEventSummaryDto> RecentHealthEvents);

public sealed record DailyActivitySummaryDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime Time,
    ActivityType ActivityType,
    string? Description);

public sealed record HealthEventSummaryDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime Time,
    HealthEventType EventType,
    string? Description);

// ─── Parent Dashboard ───────────────────────────────────────────────────────────

public sealed record ParentDashboardDto(
    Guid ParentId,
    string ParentName,

    // Children
    IReadOnlyList<ChildDashboardSummaryDto> MyChildren,

    // Today's Summary
    IReadOnlyList<ChildAttendanceTodayDto> TodaysAttendance,

    // Recent Activity
    IReadOnlyList<DailyReportSummaryDto> LatestDailyReports,
    MealsSummaryDto MealsSummary,
    NapsSummaryDto NapsSummary,
    ActivitiesSummaryDto ActivitiesSummary,

    // Notifications (placeholder for future implementation)
    IReadOnlyList<NotificationPlaceholderDto> Notifications);

public sealed record ChildDashboardSummaryDto(
    Guid Id,
    string FirstName,
    string LastName,
    string FullName,
    Guid ClassroomId,
    string? ClassroomName,
    string? PhotoUrl);

public sealed record ChildAttendanceTodayDto(
    Guid ChildId,
    string ChildName,
    AttendanceStatus Status,
    DateTime? CheckInTime,
    DateTime? CheckOutTime);

public sealed record DailyReportSummaryDto(
    Guid Id,
    Guid ChildId,
    string? ChildName,
    DateTime ReportDate,
    string? Summary,
    bool SentToParent);

public sealed record MealsSummaryDto(
    int TotalMeals,
    IReadOnlyList<MealDetailDto> RecentMeals);

public sealed record MealDetailDto(
    Guid Id,
    DateTime Time,
    FeedingType FeedingType,
    string? FoodItems,
    PortionConsumed PortionConsumed);

public sealed record NapsSummaryDto(
    int TotalNaps,
    int? AverageDurationMinutes,
    IReadOnlyList<NapDetailDto> RecentNaps);

public sealed record NapDetailDto(
    Guid Id,
    DateTime StartTime,
    DateTime? EndTime,
    NapQuality Quality,
    int? DurationMinutes);

public sealed record ActivitiesSummaryDto(
    int TotalActivities,
    IReadOnlyList<ActivityDetailDto> RecentActivities);

public sealed record ActivityDetailDto(
    Guid Id,
    DateTime Time,
    ActivityType ActivityType,
    string? Description,
    ParticipationLevel Participation);

public sealed record NotificationPlaceholderDto(
    Guid Id,
    string Title,
    string? Message,
    DateTime CreatedAt,
    bool IsRead);
