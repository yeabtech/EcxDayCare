using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECX_Daycare.Common;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Staff.Commands;
using Daycare.Application.Features.Staff.Queries;

namespace ECX_Daycare.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAdmin")]
public class StaffController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IMessageBus messageBus, ILogger<StaffController> logger)
    {
        _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a paginated list of staff members with optional filtering by role and classroom.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<StaffListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? classroomId = null,
        [FromQuery] string? role = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = new GetAllStaffQuery(page, pageSize, classroomId, role);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<StaffListResponse>.Success(result.Value, "Staff members retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff members");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    /// <summary>
    /// Searches staff members by term (name, email, phone, certifications).
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<StaffListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(ApiResponse<object>.Fail("Search term is required"));

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = new SearchStaffQuery(q.Trim(), page, pageSize);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<StaffListResponse>.Success(result.Value, "Staff search completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching staff members with term: {Term}", q);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    /// <summary>
    /// Gets a staff member by ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StaffDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetStaffByIdQuery(id);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return result.IsFailure
                ? NotFound(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<StaffDto>.Success(result.Value, "Staff member retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff member {StaffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    /// <summary>
    /// Gets all staff members assigned to a specific classroom.
    /// </summary>
    [HttpGet("classroom/{classroomId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<StaffDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByClassroom(Guid classroomId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetStaffByClassroomQuery(classroomId);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<IEnumerable<StaffDto>>.Success(result.Value, "Classroom staff retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving staff for classroom {ClassroomId}", classroomId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    /// <summary>
    /// Creates a new staff member.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StaffDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStaffRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request data"));

            var command = new CreateStaffCommand(
                request.FirstName,
                request.LastName,
                request.Role,
                request.Phone,
                request.Email,
                request.HireDate,
                request.ShiftStart,
                request.ShiftEnd,
                request.ClassroomId,
                request.Certifications);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : CreatedAtAction(
                    nameof(GetById),
                    new { id = result.Value.Id },
                    ApiResponse<StaffDto>.Success(result.Value, "Staff member created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating staff member");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    /// <summary>
    /// Updates an existing staff member.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStaffRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request data"));

            var command = new UpdateStaffCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Role,
                request.Phone,
                request.Email,
                request.HireDate,
                request.ShiftStart,
                request.ShiftEnd,
                request.ClassroomId,
                request.Certifications);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error));
            }

            return Ok(ApiResponse<object>.Success(null, "Staff member updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating staff member {StaffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    /// <summary>
    /// Deletes a staff member.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteStaffCommand(id);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error));
            }

            return Ok(ApiResponse<object>.Success(null, "Staff member deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting staff member {StaffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    /// <summary>
    /// Assigns or unassigns a staff member to/from a classroom.
    /// </summary>
    [HttpPut("{id}/assign-classroom")]
    [HttpPost("{id}/assign-classroom")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignClassroom(Guid id, [FromBody] AssignClassroomRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new AssignStaffToClassroomCommand(id, request.ClassroomId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
            {
                return result.Error.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? NotFound(ApiResponse<object>.Fail(result.Error))
                    : BadRequest(ApiResponse<object>.Fail(result.Error));
            }

            var message = request.ClassroomId.HasValue
                ? "Staff member assigned to classroom successfully"
                : "Staff member unassigned from classroom successfully";

            return Ok(ApiResponse<object>.Success(null, message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning classroom to staff member {StaffId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }
}
