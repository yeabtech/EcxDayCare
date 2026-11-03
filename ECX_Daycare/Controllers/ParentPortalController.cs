using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using ECX_Daycare.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECX_Daycare.Controllers;

/// <summary>Parent views and report read-state updates scoped to children linked to the signed-in parent account.</summary>
[ApiController]
[Route("api/parent-portal")]
[Authorize(Policy = "RequireParent")]
public sealed class ParentPortalController : ControllerBase
{
    private readonly IParentRepository _parents;
    private readonly IChildRepository _children;
    private readonly IClassroomRepository _classrooms;
    private readonly IStaffRepository _staff;
    private readonly IAttendanceRepository _attendance;
    private readonly IDailyReportRepository _reports;
    private readonly IIncidentRepository _incidents;
    private readonly IHealthLogRepository _health;
    private readonly IActivityLogRepository _activities;
    private readonly IFeedingLogRepository _feeding;
    private readonly INapLogRepository _naps;
    private readonly IEnrollmentRepository _enrollments;
    private readonly IParentNotificationRepository _notifications;
    private readonly IParentIncidentNotificationRepository _incidentNotifications;

    public ParentPortalController(
        IParentRepository parents,
        IChildRepository children,
        IClassroomRepository classrooms,
        IStaffRepository staff,
        IAttendanceRepository attendance,
        IDailyReportRepository reports,
        IIncidentRepository incidents,
        IHealthLogRepository health,
        IActivityLogRepository activities,
        IFeedingLogRepository feeding,
        INapLogRepository naps,
        IEnrollmentRepository enrollments,
        IParentNotificationRepository notifications,
        IParentIncidentNotificationRepository incidentNotifications)
    {
        _parents = parents;
        _children = children;
        _classrooms = classrooms;
        _staff = staff;
        _attendance = attendance;
        _reports = reports;
        _incidents = incidents;
        _health = health;
        _activities = activities;
        _feeding = feeding;
        _naps = naps;
        _enrollments = enrollments;
        _notifications = notifications;
        _incidentNotifications = incidentNotifications;
    }

    /// <summary>Returns in-app notifications for the authenticated parent.</summary>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ParentNotificationDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications(CancellationToken cancellationToken)
    {
        var parent = await GetParentAsync(cancellationToken);
        if (parent is null) return Forbid();

        var notifications = await _notifications.GetByParentIdAsync(parent.ParentId, cancellationToken);
        var response = notifications
            .Where(notification => notification.DailyReport is not null)
            .Select(notification => new ParentNotificationDto(
                notification.Id.Value,
                notification.DailyReportId.Value,
                notification.DailyReport!.ChildId.Value,
                notification.DailyReport.Child?.Name.FullName,
                notification.Title,
                notification.Message,
                notification.IsRead,
                new DateTimeOffset(DateTime.SpecifyKind(notification.CreatedAt, DateTimeKind.Utc)),
                notification.ReadAt.HasValue
                    ? new DateTimeOffset(DateTime.SpecifyKind(notification.ReadAt.Value, DateTimeKind.Utc))
                    : null,
                new DateTimeOffset(DateTime.SpecifyKind(notification.DailyReport.Date, DateTimeKind.Utc))))
            .ToList();

        return Ok(ApiResponse<IReadOnlyList<ParentNotificationDto>>.Success(response));
    }

    /// <summary>Returns incident reports explicitly shared with the authenticated parent.</summary>
    [HttpGet("incident-notifications")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ParentIncidentNotificationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetMyIncidentNotifications(CancellationToken cancellationToken)
    {
        var parent = await GetParentAsync(cancellationToken);
        if (parent is null) return Forbid();

        var notifications = await _incidentNotifications.GetByParentIdAsync(parent.ParentId, cancellationToken);
        var response = notifications
            .Where(notification => notification.Incident is not null)
            .Select(notification =>
            {
                var incident = notification.Incident!;
                return new ParentIncidentNotificationDto(
                    notification.Id.Value,
                    incident.Id.Value,
                    incident.ChildId.Value,
                    incident.Child?.Name.FullName ?? "Your child",
                    incident.IncidentTime,
                    incident.Location,
                    incident.Description,
                    incident.InjuryType.ToString(),
                    incident.AffectedBodyPart.ToString(),
                    incident.FirstAidGiven,
                    incident.FollowUp,
                    incident.Notes,
                    notification.IsRead,
                    new DateTimeOffset(DateTime.SpecifyKind(notification.CreatedAt, DateTimeKind.Utc)),
                    notification.ReadAt.HasValue
                        ? new DateTimeOffset(DateTime.SpecifyKind(notification.ReadAt.Value, DateTimeKind.Utc))
                        : null);
            })
            .ToList();

        return Ok(ApiResponse<IReadOnlyList<ParentIncidentNotificationDto>>.Success(response));
    }

    [HttpGet("children")]
    public async Task<IActionResult> GetMyChildren(CancellationToken cancellationToken)
    {
        var parent = await GetParentAsync(cancellationToken);
        if (parent is null) return Forbid();

        var result = new List<ParentChildProfileDto>();
        foreach (var link in parent.ParentChildren)
        {
            var child = await _children.GetByIdAsync(link.ChildId, cancellationToken);
            if (child is null) continue;
            result.Add(new ParentChildProfileDto(child.Id.Value, child.Name.FirstName, child.Name.LastName,
                child.DateOfBirth.Date, child.DateOfBirth.Age, child.Gender.ToString(), child.PhotoUrl,
                child.EnrollmentDate, child.Status.ToString(), child.CreatedAt, child.UpdatedAt,
                string.IsNullOrWhiteSpace(child.Allergies) ? null : child.Allergies.Trim(),
                string.IsNullOrWhiteSpace(child.MedicalNotes) ? null : child.MedicalNotes.Trim(),
                child.ClassroomId?.Value));
        }

        return Ok(ApiResponse<ParentChildrenProfileDto>.Success(
            new ParentChildrenProfileDto(parent.Name.FullName, result)));
    }

    [HttpGet("children/{childId:guid}")]
    public async Task<IActionResult> GetChildOverview(Guid childId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var parent = await GetParentAsync(cancellationToken);
        if (parent is null) return Forbid();
        if (!parent.ParentChildren.Any(link => link.ChildId.Value == childId)) return NotFound();

        var child = await _children.GetByIdAsync(ChildId.Create(childId), cancellationToken);
        if (child is null) return NotFound();

        var activeEnrollment = (await _enrollments.GetByChildIdAsync(ChildId.Create(childId), cancellationToken))
            .Where(enrollment => enrollment.Status == EnrollmentStatus.Active
                && enrollment.StartDate.Date <= DateTime.UtcNow.Date
                && (!enrollment.EndDate.HasValue || enrollment.EndDate.Value.Date >= DateTime.UtcNow.Date))
            .OrderByDescending(enrollment => enrollment.StartDate)
            .FirstOrDefault();
        var classroomId = child.ClassroomId?.Value ?? activeEnrollment?.ClassroomId?.Value;
        var classroom = classroomId.HasValue
            ? await _classrooms.GetByIdAsync(ClassroomId.Create(classroomId.Value), cancellationToken)
            : null;
        var leadTeacher = classroom?.LeadTeacherId is Guid teacherId
            ? await _staff.GetByIdAsync(StaffId.Create(teacherId), cancellationToken)
            : null;
        var childKey = ChildId.Create(childId);
        var startDate = from?.Date;
        var endDate = to?.Date.AddDays(1).AddTicks(-1);
        var selectedDay = from?.Date ?? DateTime.UtcNow.Date;
        var todayAttendance = await _attendance.GetTodayByChildIdAsync(childKey, selectedDay, cancellationToken);
        var reports = (await _reports.GetByChildIdAsync(childKey, page: 1, pageSize: 500, cancellationToken: cancellationToken))
            .Where(report => report.SentToParent && (!startDate.HasValue || report.Date >= startDate) && (!endDate.HasValue || report.Date <= endDate))
            .OrderByDescending(report => report.Date)
            .Select(report => new ParentDailyReportDto(report.Id.Value, report.Date, report.Mood,
                report.MealsSummary, report.NapSummary, report.ActivitiesSummary, report.DiaperSummary,
                report.Milestones, report.Concerns, report.SentToParent, report.SentAt))
            .ToList();
        var incidents = (await _incidents.GetByChildIdAsync(childKey, cancellationToken: cancellationToken))
            .Where(item => (!startDate.HasValue || item.IncidentTime >= startDate) && (!endDate.HasValue || item.IncidentTime <= endDate))
            .OrderByDescending(item => item.IncidentTime)
            .Select(item => new ParentIncidentDto(item.Id.Value, item.IncidentTime, item.Location,
                item.Description, item.InjuryType.ToString(), item.AffectedBodyPart.ToString(),
                item.FirstAidGiven, item.FollowUp, item.ParentNotified))
            .ToList();
        var healthLogs = (await _health.GetByChildIdAsync(childKey, cancellationToken: cancellationToken))
            .Where(item => (!startDate.HasValue || item.Time >= startDate) && (!endDate.HasValue || item.Time <= endDate))
            .OrderByDescending(item => item.Time)
            .Select(item => new ParentHealthDto(item.Id.Value, item.Time, item.EventType.ToString(), item.Description, item.Notes))
            .ToList();
        var attendanceHistory = (await _attendance.GetChildAttendanceHistoryAsync(childKey, startDate, endDate, page: 1, pageSize: 500, cancellationToken: cancellationToken))
            .OrderByDescending(item => item.Date)
            .Select(item => new ParentAttendanceDto(item.Date, item.Status.ToString(), item.CheckInTime, item.CheckOutTime))
            .ToList();
        var today = selectedDay;
        var meals = (from.HasValue || to.HasValue
                ? await _feeding.GetByChildIdAsync(childKey, startDate, endDate, cancellationToken)
                : await _feeding.GetByChildAndDateAsync(childKey, today, cancellationToken))
            .OrderBy(item => item.Time)
            .Select(item => new ParentMealDto(item.Time, item.FeedingType.ToString(), item.FoodItems, item.Amount, item.PortionConsumed.ToString()))
            .ToList();
        var naps = (from.HasValue || to.HasValue
                ? await _naps.GetByChildIdAsync(childKey, startDate, endDate, cancellationToken)
                : await _naps.GetByChildAndDateAsync(childKey, today, cancellationToken))
            .OrderBy(item => item.StartTime)
            .Select(item => new ParentNapDto(item.StartTime, item.EndTime, item.Quality.ToString(), item.DurationMinutes))
            .ToList();
        var activities = (from.HasValue || to.HasValue
                ? await _activities.GetByChildIdAsync(childKey, startDate, endDate, cancellationToken)
                : await _activities.GetByChildAndDateAsync(childKey, today, cancellationToken))
            .OrderBy(item => item.Time)
            .Select(item => new ParentActivityDto(item.Time, item.Title, item.Description, item.Participation.ToString()))
            .ToList();

        var overview = new ParentChildOverviewDto(
            new ParentChildDto(child.Id.Value, child.Name.FirstName, child.Name.LastName,
                child.DateOfBirth.Date, child.PhotoUrl, classroomId, classroom?.Name,
                child.Gender.ToString(),
                string.IsNullOrWhiteSpace(child.Allergies) ? null : child.Allergies.Trim(),
                string.IsNullOrWhiteSpace(child.MedicalNotes) ? null : child.MedicalNotes.Trim()),
            classroom is null ? null : new ParentClassroomDto(classroom.Id.Value, classroom.Name, classroom.Description, classroom.Status.ToString(), classroom.AgeGroup.MinAgeMonths, classroom.AgeGroup.MaxAgeMonths,
                leadTeacher is null ? null : new ParentTeacherDto(leadTeacher.FirstName, leadTeacher.LastName, leadTeacher.Email, leadTeacher.Phone)),
            todayAttendance is null ? null : new ParentAttendanceDto(todayAttendance.Date, todayAttendance.Status.ToString(), todayAttendance.CheckInTime, todayAttendance.CheckOutTime),
            attendanceHistory,
            reports,
            incidents,
            healthLogs,
            meals,
            naps,
            activities);
        return Ok(ApiResponse<ParentChildOverviewDto>.Success(overview));
    }

    [HttpPost("reports/{reportId:guid}/read")]
    public async Task<IActionResult> MarkReportRead(Guid reportId, CancellationToken cancellationToken)
    {
        var parent = await GetParentAsync(cancellationToken);
        if (parent is null) return Forbid();

        var report = await _reports.GetByIdAsync(DailyReportId.Create(reportId), cancellationToken);
        if (report is null || !parent.ParentChildren.Any(link => link.ChildId.Value == report.ChildId.Value)) return NotFound();
        if (!report.SentToParent) return BadRequest(ApiResponse<object>.Fail("This report has not been sent to the parent."));

        var markedRead = await _notifications.MarkDailyReportAsReadAsync(
            parent.ParentId,
            report.Id,
            cancellationToken);
        if (!markedRead)
            return NotFound(ApiResponse<object>.Fail("No notification exists for this report."));

        return Ok(ApiResponse<object>.Success(new { reportId, milestones = report.Milestones }));
    }

    /// <summary>Marks an incident report notification as read for the authenticated parent.</summary>
    [HttpPost("incidents/{incidentId:guid}/read")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MarkIncidentRead(Guid incidentId, CancellationToken cancellationToken)
    {
        var parent = await GetParentAsync(cancellationToken);
        if (parent is null) return Forbid();

        var markedRead = await _incidentNotifications.MarkAsReadAsync(
            parent.ParentId,
            IncidentId.Create(incidentId),
            cancellationToken);
        if (!markedRead)
            return NotFound(ApiResponse<object>.Fail("No incident notification exists for this parent."));

        return Ok(ApiResponse<object>.Success(new { incidentId }));
    }

    private async Task<Parent?> GetParentAsync(CancellationToken cancellationToken)
    {
        var parentIdValue = User.FindFirst("parentId")?.Value;
        if (!Guid.TryParse(parentIdValue, out var parentId) || parentId == Guid.Empty) return null;
        return await _parents.GetByIdAsync(ParentId.Create(parentId), cancellationToken);
    }
}

public sealed record ParentChildDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    string? PhotoUrl,
    Guid? ClassroomId,
    string? ClassroomName,
    string Gender,
    string? Allergies,
    string? MedicalNotes);
public sealed record ParentChildrenProfileDto(string ParentName, IReadOnlyList<ParentChildProfileDto> Children);
/// <summary>All scalar child profile fields returned to a linked parent.</summary>
public sealed record ParentChildProfileDto(
    Guid Id,
    string FirstName,
    string LastName,
    DateTime DateOfBirth,
    int Age,
    string Gender,
    string? PhotoUrl,
    DateTime EnrollmentDate,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? Allergies,
    string? MedicalNotes,
    Guid? ClassroomId);
public sealed record ParentClassroomDto(Guid Id, string Name, string? Description, string Status, int MinAgeMonths, int MaxAgeMonths, ParentTeacherDto? LeadTeacher);
public sealed record ParentTeacherDto(string FirstName, string LastName, string Email, string Phone);
public sealed record ParentAttendanceDto(DateTime Date, string Status, DateTime? CheckInTime, DateTime? CheckOutTime);
public sealed record ParentDailyReportDto(Guid Id, DateTime Date, string? Mood, string? MealsSummary, string? NapSummary, string? ActivitiesSummary, string? DiaperSummary, string? Milestones, string? Concerns, bool SentToParent, DateTime? SentAt);
public sealed record ParentIncidentDto(Guid Id, DateTime IncidentTime, string Location, string Description, string InjuryType, string AffectedBodyPart, string? FirstAidGiven, string? FollowUp, bool ParentNotified);
public sealed record ParentHealthDto(Guid Id, DateTime Time, string EventType, string? Description, string? Notes);
public sealed record ParentMealDto(DateTime Time, string FeedingType, string? FoodItems, string? Amount, string PortionConsumed);
public sealed record ParentNapDto(DateTime StartTime, DateTime? EndTime, string Quality, int? DurationMinutes);
public sealed record ParentActivityDto(DateTime Time, string Title, string? Description, string Participation);
public sealed record ParentChildOverviewDto(ParentChildDto Child, ParentClassroomDto? Classroom, ParentAttendanceDto? TodayAttendance, IReadOnlyList<ParentAttendanceDto> AttendanceHistory, IReadOnlyList<ParentDailyReportDto> DailyReports, IReadOnlyList<ParentIncidentDto> Incidents, IReadOnlyList<ParentHealthDto> HealthEvents, IReadOnlyList<ParentMealDto> Meals, IReadOnlyList<ParentNapDto> Naps, IReadOnlyList<ParentActivityDto> Activities);
/// <summary>In-app notification details and the report they open.</summary>
public sealed record ParentNotificationDto(Guid Id, Guid DailyReportId, Guid ChildId, string? ChildName, string Title, string Message, bool IsRead, DateTimeOffset CreatedAt, DateTimeOffset? ReadAt, DateTimeOffset ReportDate);
/// <summary>Incident report data and its parent-specific read state.</summary>
public sealed record ParentIncidentNotificationDto(
    Guid Id,
    Guid IncidentId,
    Guid ChildId,
    string ChildName,
    DateTime IncidentTime,
    string Location,
    string Description,
    string InjuryType,
    string AffectedBodyPart,
    string? FirstAidGiven,
    string? FollowUp,
    string? Notes,
    bool IsRead,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ReadAt);
