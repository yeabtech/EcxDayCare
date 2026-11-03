using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECX_Daycare.Common;
using Daycare.Application.DTOs;
using Daycare.Application.Features.DailyReports.Commands;
using Daycare.Application.Features.DailyReports.Queries;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Controller for managing daily child activity reports.
/// Staff can create and edit reports; parents can only view reports for their own children.
/// All business rules are enforced in the Application layer — this controller is intentionally thin.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class DailyReportsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<DailyReportsController> _logger;

    public DailyReportsController(IMessageBus messageBus, ILogger<DailyReportsController> logger)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─── Staff: Create ────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a new daily report for a child. Only accessible to Staff and Admins.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDailyReportRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new CreateDailyReportCommand(
                request.ChildId,
                request.Date,
                staffId,
                request.Mood,
                request.MealsSummary,
                request.NapSummary,
                request.ActivitiesSummary,
                request.DiaperSummary,
                request.Milestones,
                request.Concerns);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error!));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                ApiResponse<DailyReportDto>.Success(result.Value, "Daily report created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating daily report for child {ChildId}", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Staff: Update ────────────────────────────────────────────────────────

    /// <summary>
    /// Updates an existing daily report. Only accessible to Staff and Admins.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateDailyReportRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new UpdateDailyReportCommand(
                id,
                staffId,
                request.Mood,
                request.MealsSummary,
                request.NapSummary,
                request.ActivitiesSummary,
                request.DiaperSummary,
                request.Milestones,
                request.Concerns);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<DailyReportDto>.Success(result.Value!, "Daily report updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating daily report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Staff: Send to Parent ────────────────────────────────────────────────

    /// <summary>
    /// Marks a daily report as sent to the parent. Only accessible to Staff and Admins.
    /// </summary>
    [HttpPost("{id:guid}/send")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SendToParent([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new SendReportToParentCommand(id, staffId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<DailyReportDto>.Success(result.Value!, "Report marked as sent to parent."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending report {ReportId} to parent", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Staff: Delete ────────────────────────────────────────────────────────

    /// <summary>
    /// Deletes a daily report. Cannot delete a report that has already been sent to a parent.
    /// Only accessible to Staff and Admins.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new DeleteDailyReportCommand(id, staffId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<object>.Success(null, "Daily report deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: Single ─────────────────────────────────────────────────────────

    /// <summary>
    /// Retrieves a daily report by ID.
    /// Staff and Admin can view any report. Parents can only view reports belonging to their children.
    /// </summary>
    [HttpGet("logs/child/{childId:guid}")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DailyLogsSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyLogs(
        [FromRoute] Guid childId,
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetDailyLogsSummaryQuery(childId, date ?? DateTime.UtcNow.Date);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<DailyLogsSummaryDto>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily logs for child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DailyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var parentId = GetParentIdIfCallerIsParent();
            var query = new GetDailyReportByIdQuery(id, parentId);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                if (result.Error!.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(result.Error));

                return NotFound(ApiResponse<object>.Fail(result.Error));
            }

            return Ok(ApiResponse<DailyReportDto>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: Child Reports ──────────────────────────────────────────────────

    /// <summary>
    /// Retrieves paginated daily reports for a specific child.
    /// Staff and Admin can query any child. Parents can only view reports for their own children.
    /// </summary>
    [HttpGet("child/{childId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ChildDailyReportsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByChild(
        [FromRoute] Guid childId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var parentId = GetParentIdIfCallerIsParent();
            var query = new GetChildDailyReportsQuery(childId, startDate, endDate, page, pageSize, parentId);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                if (result.Error!.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase))
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(result.Error));

                return NotFound(ApiResponse<object>.Fail(result.Error));
            }

            return Ok(ApiResponse<ChildDailyReportsDto>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily reports for child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: Daily Feed ─────────────────────────────────────────────────────

    /// <summary>
    /// Retrieves all daily reports for a given date (defaults to today).
    /// Intended for the staff daily reporting dashboard.
    /// </summary>
    [HttpGet("daily")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<DailyReportDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailyFeed([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetDailyReportsByDateQuery(date);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<IReadOnlyList<DailyReportDto>>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily reports feed for date {Date}", date);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private Guid GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
    }

    private Guid? GetParentIdIfCallerIsParent()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        if (string.Equals(role, "Parent", StringComparison.OrdinalIgnoreCase))
        {
            var parentIdClaim = User.FindFirst("parentId")?.Value;
            if (Guid.TryParse(parentIdClaim, out var pId))
                return pId;

            var subClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (Guid.TryParse(subClaim, out var sId))
                return sId;
        }
        return null;
    }
}
