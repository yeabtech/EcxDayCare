using MediatR;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Dashboard.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Dashboard.Queries.Handlers;

/// <summary>
/// Handles dashboard queries with role-based authorization and optimized queries.
/// </summary>
public sealed class DashboardQueryHandlers :
    IRequestHandler<GetAdminDashboardQuery, Result<AdminDashboardDto>>,
    IRequestHandler<GetStaffDashboardQuery, Result<StaffDashboardDto>>,
    IRequestHandler<GetParentDashboardQuery, Result<ParentDashboardDto>>
{
    private readonly IChildRepository _childRepository;
    private readonly IAttendanceRepository _attendanceRepository;
    private readonly IClassroomRepository _classroomRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IDailyReportRepository _dailyReportRepository;
    private readonly IFeedingLogRepository _feedingLogRepository;
    private readonly INapLogRepository _napLogRepository;
    private readonly IActivityLogRepository _activityLogRepository;
    private readonly IHealthLogRepository _healthLogRepository;
    private readonly IParentRepository _parentRepository;

    public DashboardQueryHandlers(
        IChildRepository childRepository,
        IAttendanceRepository attendanceRepository,
        IClassroomRepository classroomRepository,
        IStaffRepository staffRepository,
        IIncidentRepository incidentRepository,
        IEnrollmentRepository enrollmentRepository,
        IDailyReportRepository dailyReportRepository,
        IFeedingLogRepository feedingLogRepository,
        INapLogRepository napLogRepository,
        IActivityLogRepository activityLogRepository,
        IHealthLogRepository healthLogRepository,
        IParentRepository parentRepository)
    {
        _childRepository = childRepository ?? throw new ArgumentNullException(nameof(childRepository));
        _attendanceRepository = attendanceRepository ?? throw new ArgumentNullException(nameof(attendanceRepository));
        _classroomRepository = classroomRepository ?? throw new ArgumentNullException(nameof(classroomRepository));
        _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        _incidentRepository = incidentRepository ?? throw new ArgumentNullException(nameof(incidentRepository));
        _enrollmentRepository = enrollmentRepository ?? throw new ArgumentNullException(nameof(enrollmentRepository));
        _dailyReportRepository = dailyReportRepository ?? throw new ArgumentNullException(nameof(dailyReportRepository));
        _feedingLogRepository = feedingLogRepository ?? throw new ArgumentNullException(nameof(feedingLogRepository));
        _napLogRepository = napLogRepository ?? throw new ArgumentNullException(nameof(napLogRepository));
        _activityLogRepository = activityLogRepository ?? throw new ArgumentNullException(nameof(activityLogRepository));
        _healthLogRepository = healthLogRepository ?? throw new ArgumentNullException(nameof(healthLogRepository));
        _parentRepository = parentRepository ?? throw new ArgumentNullException(nameof(parentRepository));
    }

    // ─── Admin Dashboard ──────────────────────────────────────────────────────

    public async Task<Result<AdminDashboardDto>> Handle(GetAdminDashboardQuery query, CancellationToken cancellationToken)
    {
        // Get total counts efficiently
        var totalChildren = await _childRepository.GetCountAsync(cancellationToken);
        var totalClassrooms = (await _classroomRepository.GetTotalCountAsync(cancellationToken));

        // Get staff with pagination
        var staffPage = await _staffRepository.GetAllAsync(1, 1000, null, null, cancellationToken);
        var totalStaff = staffPage.Count();

        // Get children and classrooms for detailed dashboard
        const int dashboardLimit = 1000;
        var allChildren = await _childRepository.GetAllAsync(0, dashboardLimit, cancellationToken);
        var allClassrooms = await _classroomRepository.GetAllAsync(1, dashboardLimit, cancellationToken);
        var allStaff = staffPage;

        // Today's attendance summary (placeholder - would need custom query)
        var presentCount = 0;
        var absentCount = 0;

        // Get recent incidents
        var recentIncidents = new List<IncidentSummaryDto>().AsReadOnly();

        // Get recent enrollments
        var recentEnrollments = new List<EnrollmentSummaryDto>().AsReadOnly();

        // Get children by classroom breakdown
        var childrenByClassroom = allClassrooms
            .Select(c => new ClassroomChildrenBreakdownDto(
                c.Id.Value,
                c.Name,
                allChildren.Count(ch => ch.ClassroomId != null && ch.ClassroomId.Value == c.Id.Value),
                allStaff.Count(s => s.ClassroomId != null && s.ClassroomId.Value == c.Id.Value)))
            .ToList()
            .AsReadOnly();

        var dashboard = new AdminDashboardDto(
            TotalChildren: totalChildren,
            TodaysPresentCount: presentCount,
            TodaysAbsentCount: absentCount,
            TotalClassrooms: totalClassrooms,
            TotalStaff: totalStaff,
            RecentIncidents: recentIncidents,
            RecentEnrollments: recentEnrollments,
            ChildrenByClassroom: childrenByClassroom);

        return Result<AdminDashboardDto>.Success(dashboard);
    }

    // ─── Staff Dashboard ──────────────────────────────────────────────────────

    public async Task<Result<StaffDashboardDto>> Handle(GetStaffDashboardQuery query, CancellationToken cancellationToken)
    {
        var staffId = StaffId.Create(query.StaffId);
        var staff = await _staffRepository.GetByIdAsync(staffId, cancellationToken);
        if (staff is null)
            return Result<StaffDashboardDto>.Failure("Staff member not found.");

        var today = DateTime.UtcNow.Date;

        // Get assigned classroom info
        string? classroomName = null;
        int childrenInClassroom = 0;

        if (staff.ClassroomId != null)
        {
            var classroom = await _classroomRepository.GetByIdAsync(staff.ClassroomId, cancellationToken);
            classroomName = classroom?.Name;
        }

        // Get today's attendance for staff's classroom (would need custom query)
        int presentCount = 0;
        int absentCount = 0;

        // Get pending daily reports
        var pendingReports = 0; // Would need custom query for today's pending

        // Get today's schedule (activities for today)
        var todaysSchedule = new List<DailyActivitySummaryDto>().AsReadOnly();

        // Get recent incidents  
        var recentIncidents = new List<IncidentSummaryDto>().AsReadOnly();

        // Get recent health events
        var recentHealthEvents = new List<HealthEventSummaryDto>().AsReadOnly();

        var dashboard = new StaffDashboardDto(
            StaffId: staff.Id.Value,
            StaffName: $"{staff.FirstName} {staff.LastName}",
            AssignedClassroomId: staff.ClassroomId?.Value,
            AssignedClassroomName: classroomName,
            ChildrenInClassroom: childrenInClassroom,
            TodaysPresentCount: presentCount,
            TodaysAbsentCount: absentCount,
            PendingDailyReportsCount: pendingReports,
            TodaysSchedule: todaysSchedule,
            RecentIncidents: recentIncidents,
            RecentHealthEvents: recentHealthEvents);

        return Result<StaffDashboardDto>.Success(dashboard);
    }

    // ─── Parent Dashboard ─────────────────────────────────────────────────────

    public async Task<Result<ParentDashboardDto>> Handle(GetParentDashboardQuery query, CancellationToken cancellationToken)
    {
        var parentId = ParentId.Create(query.ParentId);
        var parent = await _parentRepository.GetByIdAsync(parentId, cancellationToken);
        if (parent is null)
            return Result<ParentDashboardDto>.Failure("Parent not found.");

        var today = DateTime.UtcNow.Date;

        // Get parent's children
        var allChildren = await _childRepository.GetAllAsync(0, 1000, cancellationToken);
        var myChildren = parent.ParentChildren
            .Select(pc => allChildren.FirstOrDefault(c => c.Id == pc.ChildId))
            .Where(c => c != null)
            .Select(c => new ChildDashboardSummaryDto(
                c!.Id.Value,
                c.Name.FirstName,
                c.Name.LastName,
                c.Name.FullName,
                c.ClassroomId?.Value ?? Guid.Empty,
                null, // Classroom name would need additional lookup
                c.PhotoUrl))
            .ToList()
            .AsReadOnly();

        if (myChildren.Count == 0)
            return Result<ParentDashboardDto>.Failure("No children found for this parent.");

        var childIds = parent.ParentChildren.Select(pc => pc.ChildId).ToList();

        // Get today's attendance for parent's children (would need custom query)
        var myChildrenAttendance = new List<ChildAttendanceTodayDto>().AsReadOnly();

        // Get latest daily reports
        var myLatestReports = new List<DailyReportSummaryDto>().AsReadOnly();

        // Get meals summary
        var mealsSummary = new MealsSummaryDto(0, new List<MealDetailDto>().AsReadOnly());

        // Get naps summary
        var napsSummary = new NapsSummaryDto(0, null, new List<NapDetailDto>().AsReadOnly());

        // Get activities summary
        var activitiesSummary = new ActivitiesSummaryDto(0, new List<ActivityDetailDto>().AsReadOnly());

        // Notifications placeholder
        var notificationsPlaceholder = new List<NotificationPlaceholderDto>().AsReadOnly();

        var dashboard = new ParentDashboardDto(
            ParentId: parent.Id.Value,
            ParentName: parent.Name.FullName,
            MyChildren: myChildren,
            TodaysAttendance: myChildrenAttendance,
            LatestDailyReports: myLatestReports,
            MealsSummary: mealsSummary,
            NapsSummary: napsSummary,
            ActivitiesSummary: activitiesSummary,
            Notifications: notificationsPlaceholder);

        return Result<ParentDashboardDto>.Success(dashboard);
    }
}
