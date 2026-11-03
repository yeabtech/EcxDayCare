using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using Daycare.Application.DTOs;
using Daycare.Application.Features.HealthLogs.Commands;
using Daycare.Application.Features.HealthLogs.Queries;
using Daycare.Application.Features.Dashboard.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Identity;
using ECX_Daycare.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECX_Daycare.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class HealthLogsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<HealthLogsController> _logger;
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public HealthLogsController(
        IMessageBus messageBus,
        ILogger<HealthLogsController> logger,
        IStaffRepository staffRepository,
        UserManager<ApplicationUser> userManager)
    {
        _messageBus = messageBus;
        _logger = logger;
        _staffRepository = staffRepository;
        _userManager = userManager;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _messageBus.SendAsync(
            new GetHealthLogByIdQuery(id, GetParentIdIfCallerIsParent()), cancellationToken);
        if (result.IsFailure) return ReadFailure(result.Error!);
        return Ok(ApiResponse<HealthLogDto>.Success(result.Value!));
    }

    [HttpGet("children")]
    [Authorize(Policy = "RequireParent")]
    public async Task<IActionResult> GetParentChildren(CancellationToken cancellationToken)
    {
        var parentId = GetParentIdIfCallerIsParent();
        if (!parentId.HasValue || parentId.Value == Guid.Empty) return Forbid();

        var result = await _messageBus.SendAsync(new GetParentDashboardQuery(parentId.Value), cancellationToken);
        if (result.IsFailure) return NotFound(ApiResponse<object>.Fail(result.Error));
        return Ok(ApiResponse<IReadOnlyList<ChildDashboardSummaryDto>>.Success(result.Value!.MyChildren));
    }

    [HttpGet("child/{childId:guid}")]
    public async Task<IActionResult> GetByChild(
        Guid childId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.SendAsync(
            new GetHealthLogsByChildQuery(childId, startDate, endDate, GetParentIdIfCallerIsParent()),
            cancellationToken);
        if (result.IsFailure) return ReadFailure(result.Error!);
        return Ok(ApiResponse<IReadOnlyList<HealthLogDto>>.Success(result.Value!));
    }

    [HttpGet("child/{childId:guid}/date/{date:datetime}")]
    public async Task<IActionResult> GetByChildAndDate(
        Guid childId,
        DateTime date,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.SendAsync(
            new GetHealthLogsByChildAndDateQuery(childId, date, GetParentIdIfCallerIsParent()),
            cancellationToken);
        if (result.IsFailure) return ReadFailure(result.Error!);
        return Ok(ApiResponse<IReadOnlyList<HealthLogDto>>.Success(result.Value!));
    }

    [HttpPost]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateHealthLogRequestDto request,
        CancellationToken cancellationToken)
    {
        var staffId = await GetCurrentStaffIdAsync(cancellationToken);

        try
        {
            var result = await _messageBus.SendAsync(new CreateHealthLogCommand(
                request.ChildId, request.Time, request.EventType, request.Description, staffId, request.Notes),
                cancellationToken);
            if (result.IsFailure) return BadRequest(ApiResponse<object>.Fail(result.Error));

            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
                ApiResponse<HealthLogDto>.Success(result.Value, "Health event recorded."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not record health event for child {ChildId}", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred while recording the health event."));
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateHealthLogRequestDto request,
        CancellationToken cancellationToken)
    {
        var staffId = await GetCurrentStaffIdAsync(cancellationToken);

        try
        {
            var result = await _messageBus.SendAsync(new UpdateHealthLogCommand(
                id, request.Time, request.EventType, request.Description, staffId, request.Notes),
                cancellationToken);
            if (result.IsFailure) return WriteFailure(result.Error!);
            return Ok(ApiResponse<HealthLogDto>.Success(result.Value!, "Health event updated."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not update health event {HealthLogId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred while updating the health event."));
        }
    }

    private IActionResult ReadFailure(string error) =>
        error.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase)
            ? StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(error))
            : NotFound(ApiResponse<object>.Fail(error));

    private IActionResult WriteFailure(string error) =>
        error.Contains("not found", StringComparison.OrdinalIgnoreCase)
            ? NotFound(ApiResponse<object>.Fail(error))
            : BadRequest(ApiResponse<object>.Fail(error));

    private Guid? GetParentIdIfCallerIsParent()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
        if (!string.Equals(role, "Parent", StringComparison.OrdinalIgnoreCase)) return null;

        var parentId = User.FindFirst("parentId")?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        return Guid.TryParse(parentId, out var id) ? id : Guid.Empty;
    }

    private async Task<Guid> GetCurrentStaffIdAsync(CancellationToken cancellationToken)
    {
        var userIdClaim = GetAccountSubject();
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user?.StaffId is Guid linkedStaffId &&
                await _staffRepository.GetByIdAsync(StaffId.Create(linkedStaffId), cancellationToken) is not null)
                return linkedStaffId;
        }

        if (Guid.TryParse(User.FindFirst("staffId")?.Value, out var staffId) &&
            await _staffRepository.GetByIdAsync(StaffId.Create(staffId), cancellationToken) is not null)
            return staffId;

        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? User.FindFirst("email")?.Value;
        if (!string.IsNullOrWhiteSpace(email))
        {
            var staff = await _staffRepository.GetByEmailAsync(email, cancellationToken);
            if (staff is not null) return staff.Id.Value;
        }

        // StaffId is a required legacy column without a staff foreign key. Health
        // reporting must remain available to authenticated staff/admin accounts
        // even when account linking data is incomplete.
        if (Guid.TryParse(userIdClaim, out var accountId)) return accountId;
        return CreateStableAccountId(userIdClaim ?? User.Identity?.Name ?? Guid.NewGuid().ToString());
    }

    private string? GetAccountSubject() =>
        User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? User.FindFirst("userId")?.Value
        ?? User.FindFirst("uid")?.Value;

    private static Guid CreateStableAccountId(string value) =>
        new(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value)).AsSpan(0, 16));
}
