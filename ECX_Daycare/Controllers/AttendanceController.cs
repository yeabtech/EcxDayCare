using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using ECX_Daycare.Common;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Attendance.Commands;
using Daycare.Application.Features.Attendance.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Identity;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Controller for managing daily child attendance, check-ins, check-outs, and attendance histories.
/// Thin controller delegating all domain rules and operations to CQRS handlers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AttendanceController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<AttendanceController> _logger;
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public AttendanceController(
        IMessageBus messageBus,
        ILogger<AttendanceController> logger,
        IStaffRepository staffRepository,
        UserManager<ApplicationUser> userManager)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _staffRepository = staffRepository ?? throw new ArgumentNullException(nameof(staffRepository));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
    }

    /// <summary>
    /// Checks in a child for today.
    /// </summary>
    [HttpPost("checkin")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = await GetCurrentStaffIdAsync(cancellationToken);

            var command = new CheckInChildCommand(
                ChildId: request.ChildId,
                CheckInTime: request.CheckInTime,
                ClassroomId: request.ClassroomId,
                DroppedOffBy: request.DroppedOffBy,
                Notes: request.Notes,
                Status: request.Status,
                StaffId: staffId);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Error!));
            }

            return CreatedAtAction(
                nameof(GetTodayAttendance),
                new { date = result.Value!.Date.ToString("yyyy-MM-dd") },
                ApiResponse<AttendanceDto>.Success(result.Value, "Child checked in successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in child {ChildId}", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred during check-in."));
        }
    }

    /// <summary>
    /// Checks out a child from daycare.
    /// </summary>
    [HttpPost("checkout")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = await GetCurrentStaffIdAsync(cancellationToken);

            var command = new CheckOutChildCommand(
                ChildId: request.ChildId,
                CheckOutTime: request.CheckOutTime,
                PickedUpBy: request.PickedUpBy,
                Notes: request.Notes,
                IsEarlyPickup: request.IsEarlyPickup,
                StaffId: staffId);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<AttendanceDto>.Success(result.Value!, "Child checked out successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out child {ChildId}", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred during check-out."));
        }
    }

    /// <summary>
    /// Checks out a child by specific attendance record identifier.
    /// </summary>
    [HttpPost("{id:guid}/checkout")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CheckOutById(
        [FromRoute] Guid id,
        [FromBody] CheckOutRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var staffId = await GetCurrentStaffIdAsync(cancellationToken);

            var command = new CheckOutChildCommand(
                AttendanceId: id,
                ChildId: request.ChildId,
                CheckOutTime: request.CheckOutTime,
                PickedUpBy: request.PickedUpBy,
                Notes: request.Notes,
                IsEarlyPickup: request.IsEarlyPickup,
                StaffId: staffId);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<AttendanceDto>.Success(result.Value!, "Child checked out successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking out attendance record {AttendanceId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred during check-out."));
        }
    }

    /// <summary>
    /// Marks a child absent for a specific date.
    /// </summary>
    [HttpPost("absent")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<AttendanceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkAbsent([FromBody] MarkAbsentRequestDto request, CancellationToken cancellationToken)
    {
        try
        {
            var staffId = await GetCurrentStaffIdAsync(cancellationToken);

            var command = new MarkChildAbsentCommand(
                request.ChildId,
                request.Date,
                request.Notes,
                staffId);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<AttendanceDto>.Success(result.Value!, "Child marked absent successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking child {ChildId} absent", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    /// <summary>
    /// Retrieves daily attendance dashboard summary and child records.
    /// </summary>
    [HttpGet("today")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<DailyAttendanceSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTodayAttendance(
        [FromQuery] DateTime? date,
        [FromQuery] Guid? classroomId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetTodayAttendanceQuery(date, classroomId, status);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<DailyAttendanceSummaryDto>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily attendance summary");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    /// <summary>
    /// Retrieves historical attendance records and attendance analytics for a specific child.
    /// </summary>
    [HttpGet("child/{childId:guid}/history")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<ChildAttendanceHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChildAttendanceHistory(
        [FromRoute] Guid childId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetChildAttendanceHistoryQuery(childId, startDate, endDate, page, pageSize);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                return NotFound(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<ChildAttendanceHistoryDto>.Success(result.Value!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving attendance history for child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred."));
        }
    }

    private async Task<Guid> GetCurrentStaffIdAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst("userId")?.Value
            ?? User.FindFirst("uid")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user?.StaffId is Guid linkedStaffId &&
                await _staffRepository.GetByIdAsync(StaffId.Create(linkedStaffId), cancellationToken) is not null)
            {
                return linkedStaffId;
            }
        }

        var staffIdClaim = User.FindFirst("staffId")?.Value;
        if (Guid.TryParse(staffIdClaim, out var staffId) &&
            await _staffRepository.GetByIdAsync(StaffId.Create(staffId), cancellationToken) is not null)
        {
            return staffId;
        }

        var email = User.FindFirst(ClaimTypes.Email)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Email)?.Value
            ?? User.FindFirst("email")?.Value;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var staff = await _staffRepository.GetByEmailAsync(email, cancellationToken);
            if (staff is not null) return staff.Id.Value;
        }

        // Attendance's staff ID is optional in storage. Use the authenticated
        // account ID when no linked Staff row can be resolved so check-in and
        // check-out remain available to authorized staff/admin accounts.
        if (Guid.TryParse(userIdClaim, out var accountId)) return accountId;
        var actorKey = userIdClaim ?? User.Identity?.Name ?? Guid.NewGuid().ToString();
        return new Guid(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(actorKey)).AsSpan(0, 16));
    }
}
