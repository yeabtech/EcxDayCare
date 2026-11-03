using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECX_Daycare.Common;
using Daycare.Application.DTOs;
using Daycare.Application.Features.FeedingLogs.Commands;
using Daycare.Application.Features.FeedingLogs.Queries;
using Daycare.Domain.Entities;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Controller for managing child feeding logs.
/// All endpoints require Staff or Admin authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireStaffOrAdmin")]
public sealed class FeedingLogsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<FeedingLogsController> _logger;

    public FeedingLogsController(IMessageBus messageBus, ILogger<FeedingLogsController> logger)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─── Create ───────────────────────────────────────────────────────────────

    /// <summary>Creates a new feeding log entry for a child.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<FeedingLogDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFeedingLogRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new CreateFeedingLogCommand(
                request.ChildId,
                request.Time,
                request.FeedingType,
                request.FoodItems,
                request.Amount,
                request.PortionConsumed,
                staffId,
                request.Notes);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error!));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                ApiResponse<FeedingLogDto>.Success(result.Value, "Feeding log created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating feeding log for child {ChildId}", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    /// <summary>Updates an existing feeding log entry.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FeedingLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateFeedingLogRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new UpdateFeedingLogCommand(
                id,
                request.Time,
                request.FeedingType,
                request.FoodItems,
                request.Amount,
                request.PortionConsumed,
                staffId,
                request.Notes);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<FeedingLogDto>.Success(result.Value!, "Feeding log updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feeding log {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Delete ───────────────────────────────────────────────────────────────

    /// <summary>Deletes a feeding log entry.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new DeleteFeedingLogCommand(id, staffId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<object>.Success(null, "Feeding log deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting feeding log {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: Single ─────────────────────────────────────────────────────────

    /// <summary>Retrieves a feeding log by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<FeedingLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetFeedingLogByIdQuery(id);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return NotFound(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<FeedingLogDto>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feeding log {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: By Child & Date ────────────────────────────────────────────────

    /// <summary>Retrieves all feeding logs for a child on a specific date (defaults to today).</summary>
    [HttpGet("child/{childId:guid}/daily")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FeedingLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChildAndDate(
        [FromRoute] Guid childId,
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetFeedingLogsByChildAndDateQuery(childId, date ?? DateTime.UtcNow.Date);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<IReadOnlyList<FeedingLogDto>>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feeding logs for child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: By Child (History) ─────────────────────────────────────────────

    /// <summary>Retrieves feeding history for a child with optional date range.</summary>
    [HttpGet("child/{childId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FeedingLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChild(
        [FromRoute] Guid childId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetFeedingLogsByChildQuery(childId, startDate, endDate);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<IReadOnlyList<FeedingLogDto>>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving feeding history for child {ChildId}", childId);
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
}

// ─── Request DTOs ──────────────────────────────────────────────────────────────

public sealed record CreateFeedingLogRequestDto(
    Guid ChildId,
    DateTime Time,
    FeedingType FeedingType,
    string? FoodItems,
    string? Amount,
    PortionConsumed PortionConsumed,
    string? Notes = null);

public sealed record UpdateFeedingLogRequestDto(
    DateTime Time,
    FeedingType FeedingType,
    string? FoodItems,
    string? Amount,
    PortionConsumed PortionConsumed,
    string? Notes = null);
