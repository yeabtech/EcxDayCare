using MediatR;
using Daycare.Application.DTOs;
using Daycare.Domain.Results;

namespace Daycare.Application.Features.Dashboard.Queries;

/// <summary>
/// Query for admin dashboard with system-wide overview.
/// Shows total counts, today's attendance, incidents, enrollments, and classroom breakdown.
/// </summary>
public sealed record GetAdminDashboardQuery(
    Guid? RequestingUserId = null) : IRequest<Result<AdminDashboardDto>>;

/// <summary>
/// Query for staff dashboard with classroom-focused overview.
/// Shows assigned classroom, children, today's attendance, pending reports, incidents, and health events.
/// </summary>
public sealed record GetStaffDashboardQuery(
    Guid StaffId) : IRequest<Result<StaffDashboardDto>>;

/// <summary>
/// Query for parent dashboard with child-focused overview.
/// Shows my children, today's attendance, latest reports, meals, naps, and activities.
/// Includes placeholder for notifications (future implementation).
/// </summary>
public sealed record GetParentDashboardQuery(
    Guid ParentId) : IRequest<Result<ParentDashboardDto>>;
