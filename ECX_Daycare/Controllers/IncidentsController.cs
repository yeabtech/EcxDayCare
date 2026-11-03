using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Claims;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Incidents.Commands;
using Daycare.Application.Features.Incidents.Queries;
using Daycare.Domain.Entities;
using Daycare.Domain.Repositories;
using Daycare.Infrastructure.Identity;
using ECX_Daycare.Services;
using ECX_Daycare.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ECX_Daycare.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class IncidentsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<IncidentsController> _logger;
    private readonly IStaffRepository _staffRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IIncidentRepository _incidentRepository;
    private readonly IParentRepository _parentRepository;
    private readonly IParentIncidentNotificationRepository _parentIncidentNotifications;
    private readonly IChildRepository _childRepository;
    private readonly IIncidentEmailNotificationService _incidentEmailNotifications;

    /// <summary>Initializes the incident API and its notification dependencies.</summary>
    public IncidentsController(
        IMessageBus messageBus,
        ILogger<IncidentsController> logger,
        IStaffRepository staffRepository,
        UserManager<ApplicationUser> userManager,
        IIncidentRepository incidentRepository,
        IParentRepository parentRepository,
        IParentIncidentNotificationRepository parentIncidentNotifications,
        IChildRepository childRepository,
        IIncidentEmailNotificationService incidentEmailNotifications)
    {
        _messageBus = messageBus;
        _logger = logger;
        _staffRepository = staffRepository;
        _userManager = userManager;
        _incidentRepository = incidentRepository;
        _parentRepository = parentRepository;
        _parentIncidentNotifications = parentIncidentNotifications;
        _childRepository = childRepository;
        _incidentEmailNotifications = incidentEmailNotifications;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _messageBus.SendAsync(
            new GetIncidentByIdQuery(id, GetParentIdIfCallerIsParent()), cancellationToken);
        if (result.IsFailure) return ReadFailure(result.Error!);
        return Ok(ApiResponse<IncidentDto>.Success(result.Value!));
    }

    [HttpGet("child/{childId:guid}")]
    public async Task<IActionResult> GetByChild(
        Guid childId,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        CancellationToken cancellationToken)
    {
        var result = await _messageBus.SendAsync(
            new GetIncidentsByChildQuery(childId, startDate, endDate, GetParentIdIfCallerIsParent()),
            cancellationToken);
        if (result.IsFailure) return ReadFailure(result.Error!);
        return Ok(ApiResponse<IReadOnlyList<IncidentDto>>.Success(result.Value!));
    }

    [HttpGet("child/{childId:guid}/unsigned")]
    public async Task<IActionResult> GetUnsignedByChild(Guid childId, CancellationToken cancellationToken)
    {
        var result = await _messageBus.SendAsync(
            new GetUnsignedIncidentsByChildQuery(childId, GetParentIdIfCallerIsParent()), cancellationToken);
        if (result.IsFailure) return ReadFailure(result.Error!);
        return Ok(ApiResponse<IReadOnlyList<IncidentDto>>.Success(result.Value!));
    }

    [HttpPost]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    public async Task<IActionResult> Create(
        [FromBody] CreateIncidentRequestDto request,
        CancellationToken cancellationToken)
    {
        var staffId = await GetCurrentStaffIdAsync(cancellationToken);

        Daycare.Domain.Results.Result<IncidentDto> result;
        try
        {
            result = await _messageBus.SendAsync(new CreateIncidentCommand(
                request.ChildId,
                request.IncidentTime,
                request.Location,
                request.Description,
                request.InjuryType,
                request.AffectedBodyPart,
                request.FirstAidGiven,
                staffId,
                FollowUp: request.FollowUp,
                Notes: request.Notes), cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not create incident report for child {ChildId}", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred while creating the incident report."));
        }

        if (result.IsFailure) return BadRequest(ApiResponse<object>.Fail(result.Error));

        var responseMessage = "Incident report saved. Parent email will be sent when staff marks parent notified.";
        try
        {
            var parents = await _parentRepository.GetParentsByChildAsync(
                ChildId.Create(request.ChildId),
                cancellationToken);
            await _parentIncidentNotifications.EnsureForIncidentAsync(
                IncidentId.Create(result.Value!.Id),
                parents.Select(parent => parent.ParentId).ToArray(),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Incident {IncidentId} was saved, but parent in-app alerts could not be created",
                result.Value.Id);
            responseMessage = "Incident report saved. Parent in-app alerts could not be created; email will be sent when staff marks parent notified.";
        }

        return Created($"/api/incidents/{result.Value!.Id:D}",
            ApiResponse<IncidentDto>.Success(result.Value, responseMessage));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateIncidentRequestDto request,
        CancellationToken cancellationToken)
    {
        var staffId = await GetCurrentStaffIdAsync(cancellationToken);

        try
        {
            var result = await _messageBus.SendAsync(new UpdateIncidentCommand(
                id,
                request.IncidentTime,
                request.Location,
                request.Description,
                request.InjuryType,
                request.AffectedBodyPart,
                request.FirstAidGiven,
                staffId,
                request.FollowUp,
                request.Notes), cancellationToken);
            if (result.IsFailure) return WriteFailure(result.Error!);
            return Ok(ApiResponse<IncidentDto>.Success(result.Value!, "Incident report updated."));
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Could not update incident report {IncidentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.Fail("An unexpected error occurred while updating the incident report."));
        }
    }

    /// <summary>Marks an incident as parent-notified and creates unread parent alerts.</summary>
    [HttpPost("{id:guid}/notify-parent")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> MarkParentNotified(Guid id, CancellationToken cancellationToken)
    {
        var staffId = await GetCurrentStaffIdAsync(cancellationToken);

        var incident = await _incidentRepository.GetByIdAsync(IncidentId.Create(id), cancellationToken);
        if (incident is null) return NotFound(ApiResponse<object>.Fail("Incident not found."));

        var notification = await NotifyLinkedParentsAsync(incident, staffId, cancellationToken);
        if (!notification.Succeeded)
        {
            return StatusCode(notification.FailureStatusCode, ApiResponse<object>.Fail(notification.Message));
        }

        return Ok(ApiResponse<object>.Success(true, notification.Message));
    }

    private async Task<ParentNotificationAttempt> NotifyLinkedParentsAsync(
        Incident incident,
        Guid staffId,
        CancellationToken cancellationToken)
    {
        var parents = await _parentRepository.GetParentsByChildAsync(incident.ChildId, cancellationToken);
        await _parentIncidentNotifications.EnsureForIncidentAsync(
            incident.Id,
            parents.Select(parent => parent.ParentId).ToArray(),
            cancellationToken);

        if (incident.ParentNotified)
            return ParentNotificationAttempt.Success(
                "Parent notification was already recorded; in-app alerts are up to date.");

        var recipientEmails = parents
            .Select(parent => parent.ContactInfo.Email.Trim())
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (recipientEmails.Length == 0)
            return ParentNotificationAttempt.Failed(
                "The incident was saved, but no linked parent has an email address. Add a parent email, then use Mark parent notified to retry.",
                StatusCodes.Status409Conflict);

        var child = await _childRepository.GetByIdAsync(incident.ChildId, cancellationToken);
        if (child is null)
            return ParentNotificationAttempt.Failed(
                "The incident was saved, but its child record could not be found for parent email notification.",
                StatusCodes.Status404NotFound);

        try
        {
            await _incidentEmailNotifications.SendIncidentNotificationAsync(
                incident,
                child.Name.FullName,
                recipientEmails,
                cancellationToken);
        }
        catch (IncidentEmailConfigurationException exception)
        {
            _logger.LogError(exception, "Incident email settings are missing; incident {IncidentId} was not marked as notified", incident.Id.Value);
            return ParentNotificationAttempt.Failed(
                "The incident was saved, but incident email is not configured. Fix the email settings, then use Mark parent notified to retry.",
                StatusCodes.Status503ServiceUnavailable);
        }
        catch (IncidentEmailDeliveryException exception)
        {
            _logger.LogError(exception, "Incident email delivery failed; incident {IncidentId} was not marked as notified", incident.Id.Value);
            return ParentNotificationAttempt.Failed(
                "The incident was saved, but email delivery failed. Check the email settings, then use Mark parent notified to retry.",
                StatusCodes.Status502BadGateway);
        }

        var notifyResult = await _messageBus.SendAsync(
            new NotifyParentCommand(incident.Id.Value, staffId),
            cancellationToken);
        if (notifyResult.IsFailure && !incident.ParentNotified)
        {
            _logger.LogError(
                "Email was sent for incident {IncidentId}, but recording the parent-notified status failed: {Error}",
                incident.Id.Value,
                notifyResult.Error);
            return ParentNotificationAttempt.Failed(
                "The incident email was sent, but the notification status could not be recorded. Contact an administrator before retrying.",
                StatusCodes.Status500InternalServerError);
        }

        return ParentNotificationAttempt.Success(
            "Incident report saved, parent email sent, and in-app alerts created.");
    }

    private sealed record ParentNotificationAttempt(
        bool Succeeded,
        int FailureStatusCode,
        string Message)
    {
        public static ParentNotificationAttempt Success(string message) =>
            new(true, StatusCodes.Status200OK, message);

        public static ParentNotificationAttempt Failed(string message, int failureStatusCode) =>
            new(false, failureStatusCode, message);
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

        // StaffId is a required legacy column without a staff foreign key. Incident
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
