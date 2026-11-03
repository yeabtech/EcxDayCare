using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECX_Daycare.Common;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Enrollments.Commands;
using Daycare.Application.Features.Enrollments.Queries;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Controller for managing child enrollments.
/// Thin controller delegating all business invariants to the Application & Domain CQRS pipeline.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class EnrollmentsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(IMessageBus messageBus, ILogger<EnrollmentsController> logger)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new enrollment for a child.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnrollmentDto request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateEnrollmentCommand(
                request.ChildId,
                request.ProgramType,
                request.DaysOfWeek,
                request.StartDate,
                request.EndDate,
                request.ContractSigned,
                request.ImmunizationRecord,
                request.BirthCertificate,
                request.MedicalForm,
                request.ClassroomId);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return BadRequest(ApiResponse<object>.Fail(result.Error!));
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value!.Id },
                ApiResponse<EnrollmentDto>.Success(result.Value, "Enrollment created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating enrollment for child {ChildId}", request.ChildId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Updates an existing enrollment.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateEnrollmentDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdateEnrollmentCommand(
                id,
                request.ProgramType,
                request.DaysOfWeek,
                request.StartDate,
                request.EndDate,
                request.ContractSigned,
                request.ImmunizationRecord,
                request.BirthCertificate,
                request.MedicalForm,
                request.ClassroomId,
                request.Status,
                request.EndReason);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error == "Enrollment not found"
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<EnrollmentDto>.Success(result.Value!, "Enrollment updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating enrollment {EnrollmentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Ends an ongoing enrollment.
    /// </summary>
    [HttpPost("{id:guid}/end")]
    [Authorize(Policy = "RequireStaffOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> End(
        [FromRoute] Guid id,
        [FromBody] EndEnrollmentDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new EndEnrollmentCommand(id, request.EndDate, request.Reason);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error == "Enrollment not found"
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<EnrollmentDto>.Success(result.Value!, "Enrollment ended successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending enrollment {EnrollmentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Retrieves a single enrollment by its identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetEnrollmentByIdQuery(id);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                return NotFound(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<EnrollmentDto>.Success(result.Value!, "Enrollment retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollment {EnrollmentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Retrieves a paginated list of enrollments with optional filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentListDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] string? programType = null,
        [FromQuery] Guid? classroomId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetAllEnrollmentsQuery(page, pageSize, status, programType, classroomId);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return Ok(ApiResponse<EnrollmentListDto>.Success(result.Value!, "Enrollments retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollments list");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Retrieves the enrollment history for a child (all past, current, and pending enrollments).
    /// </summary>
    [HttpGet("child/{childId:guid}/history")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EnrollmentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChildHistory([FromRoute] Guid childId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetEnrollmentsByChildIdQuery(childId);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                return NotFound(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<IReadOnlyList<EnrollmentDto>>.Success(result.Value!, "Child enrollment history retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving enrollment history for child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Retrieves the active or pending enrollment for a child.
    /// </summary>
    [HttpGet("child/{childId:guid}/active")]
    [ProducesResponseType(typeof(ApiResponse<EnrollmentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetChildActiveEnrollment([FromRoute] Guid childId, CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetActiveEnrollmentByChildIdQuery(childId);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
            {
                return NotFound(ApiResponse<object>.Fail(result.Error!));
            }

            return Ok(ApiResponse<EnrollmentDto>.Success(result.Value!, "Active enrollment retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active enrollment for child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An unexpected error occurred"));
        }
    }
}
