using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECX_Daycare.Common;
using Daycare.Application.DTOs;
using Daycare.Application.Features.NapLogs.Commands;
using Daycare.Application.Features.NapLogs.Queries;
using Daycare.Domain.Entities;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Controller for managing child nap logs.
/// All endpoints require Staff or Admin authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireStaffOrAdmin")]
public sealed class NapLogsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<NapLogsController> _logger;

    public NapLogsController(IMessageBus messageBus, ILogger<NapLogsController> logger)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ─── Create ───────────────────────────────────────────────────────────────

    /// <summary>Creates a new nap log entry for a child.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<NapLogDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateNapLogRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new CreateNapLogCommand(
                request.ChildId,
                request.StartTime,
                request.EndTime,
                request.Quality,
                staffId,
                request.Notes);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error!));

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                ApiResponse<NapLogDto>.Success(result.Value, "Nap log created successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating nap log for child {ChildId}", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Update ───────────────────────────────────────────────────────────────

    /// <summary>Updates an existing nap log entry.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NapLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateNapLogRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new UpdateNapLogCommand(
                id,
                request.StartTime,
                request.EndTime,
                request.Quality,
                staffId,
                request.Notes);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<NapLogDto>.Success(result.Value!, "Nap log updated successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating nap log {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── End Nap ──────────────────────────────────────────────────────────────

    /// <summary>Ends an active/ongoing nap by recording the end time.</summary>
    [HttpPost("{id:guid}/end")]
    [ProducesResponseType(typeof(ApiResponse<NapLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EndNap([FromRoute] Guid id, [FromBody] EndNapRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new EndNapLogCommand(id, request.EndTime, request.Quality, staffId, request.Notes);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<NapLogDto>.Success(result.Value!, "Nap ended successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending nap log {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Delete ───────────────────────────────────────────────────────────────

    /// <summary>Deletes a nap log entry.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = GetCurrentUserId();
            var command = new DeleteNapLogCommand(id, staffId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return result.Error!.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<object>.Success(null, "Nap log deleted successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting nap log {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: Single ─────────────────────────────────────────────────────────

    /// <summary>Retrieves a nap log by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<NapLogDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetNapLogByIdQuery(id);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return NotFound(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<NapLogDto>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nap log {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: By Child & Date ────────────────────────────────────────────────

    /// <summary>Retrieves all nap logs for a child on a specific date (defaults to today).</summary>
    [HttpGet("child/{childId:guid}/daily")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NapLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChildAndDate(
        [FromRoute] Guid childId,
        [FromQuery] DateTime? date,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetNapLogsByChildAndDateQuery(childId, date ?? DateTime.UtcNow.Date);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<IReadOnlyList<NapLogDto>>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nap logs for child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    // ─── Read: By Child (History) ─────────────────────────────────────────────

    /// <summary>Retrieves nap history for a child with optional date range.</summary>
    [HttpGet("child/{childId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<NapLogDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChild(
        [FromRoute] Guid childId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetNapLogsByChildQuery(childId, startDate, endDate);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error!));

            return Ok(ApiResponse<IReadOnlyList<NapLogDto>>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nap history for child {ChildId}", childId);
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

public sealed record CreateNapLogRequestDto(
    Guid ChildId,
    DateTime StartTime,
    DateTime? EndTime,
    NapQuality Quality,
    string? Notes = null);

public sealed record UpdateNapLogRequestDto(
    DateTime StartTime,
    DateTime? EndTime,
    NapQuality Quality,
    string? Notes = null);

public sealed record EndNapRequestDto(
    DateTime EndTime,
    NapQuality? Quality,
    string? Notes = null);
