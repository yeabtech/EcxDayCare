using MediatR;
using Microsoft.AspNetCore.Mvc;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Dashboard.Queries;
using Microsoft.AspNetCore.Authorization;

namespace Daycare.Controllers;

/// <summary>
/// Dashboard endpoints for role-specific views.
/// Admin, Staff, and Parent dashboards provide quick overviews using existing modules.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the admin dashboard with system-wide overview.
    /// Shows total children, today's attendance, classrooms, staff, recent incidents, recent enrollments, and classroom breakdown.
    /// </summary>
    /// <remarks>
    /// Restricted to admin users.
    /// </remarks>
    [HttpGet("admin")]
    [Authorize(Policy = "RequireAdmin")]
    [ProducesResponseType(typeof(AdminDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdminDashboard(CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetAdminDashboardQuery();
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin dashboard");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the dashboard");
        }
    }

    /// <summary>
    /// Gets the staff dashboard with classroom-focused overview.
    /// Shows assigned classroom, children, today's attendance, pending reports, schedule, incidents, and health events.
    /// </summary>
    /// <param name="staffId">The staff member's ID</param>
    /// <remarks>
    /// Restricted to the staff member themselves or admin.
    /// </remarks>
    [HttpGet("staff/{staffId}")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(StaffDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStaffDashboard(Guid staffId, CancellationToken cancellationToken)
    {
        if (staffId == Guid.Empty)
            return BadRequest("Staff ID is required");

        if (!User.IsInRole("Admin")
            && (!Guid.TryParse(User.FindFirst("staffId")?.Value, out var authenticatedStaffId)
                || authenticatedStaffId != staffId))
            return Forbid();

        try
        {
            var query = new GetStaffDashboardQuery(staffId);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff dashboard for staff {StaffId}", staffId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the dashboard");
        }
    }

    /// <summary>
    /// Gets the parent dashboard with child-focused overview.
    /// Shows parent's children, today's attendance, latest reports, meals, naps, activities, and notifications.
    /// </summary>
    /// <param name="parentId">The parent's ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <remarks>
    /// Restricted to the parent themselves or admin.
    /// </remarks>
    [HttpGet("parent/{parentId}")]
    [Authorize(Policy = "RequireParent")]
    [ProducesResponseType(typeof(ParentDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParentDashboard(Guid parentId, CancellationToken cancellationToken)
    {
        if (parentId == Guid.Empty)
            return BadRequest("Parent ID is required");

        if (!Guid.TryParse(User.FindFirst("parentId")?.Value, out var authenticatedParentId)
            || authenticatedParentId != parentId)
            return Forbid();

        try
        {
            var query = new GetParentDashboardQuery(parentId);
            var result = await _mediator.Send(query, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result.Error);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent dashboard for parent {ParentId}", parentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the dashboard");
        }
    }
}
