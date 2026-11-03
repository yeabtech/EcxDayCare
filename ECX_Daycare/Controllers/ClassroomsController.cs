using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECX_Daycare.Common;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Classrooms.Commands;
using Daycare.Application.Features.Classrooms.Queries;

namespace ECX_Daycare.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireStaffOrAdmin")]
public class ClassroomsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<ClassroomsController> _logger;

    public ClassroomsController(IMessageBus messageBus, ILogger<ClassroomsController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ClassroomListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            var query = new GetAllClassroomsQuery(page, pageSize);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<ClassroomListResponse>.Success(result.Value, "Classrooms retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classrooms");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<ClassroomListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(ApiResponse<object>.Fail("Search term is required"));

            var query = new SearchClassroomsQuery(q.Trim(), page);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<ClassroomListResponse>.Success(result.Value, "Search completed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching classrooms");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ClassroomDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetClassroomQuery(id);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return result.IsFailure
                ? NotFound(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<ClassroomDto>.Success(result.Value, "Classroom retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving classroom {ClassroomId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    [HttpGet("{id}/children")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ChildDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetChildren(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetClassroomChildrenQuery(id);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            return result.IsFailure
                ? NotFound(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<IReadOnlyList<ChildDto>>.Success(result.Value, "Classroom children retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving children for classroom {ClassroomId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ClassroomDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateClassroomRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request data"));

            var command = new CreateClassroomCommand(
                request.Name,
                request.MinAgeMonths,
                request.MaxAgeMonths,
                request.MaxCapacity,
                request.LeadTeacherId,
                request.Description);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, ApiResponse<ClassroomDto>.Success(result.Value, "Classroom created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating classroom");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateClassroomRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request data"));

            var command = new UpdateClassroomCommand(
                id,
                request.Name,
                request.MinAgeMonths,
                request.MaxAgeMonths,
                request.MaxCapacity,
                request.Status,
                request.LeadTeacherId,
                request.Description);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<object>.Success(null!, "Classroom updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating classroom {ClassroomId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteClassroomCommand(id);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            return result.IsFailure
                ? NotFound(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<object>.Success(null!, "Classroom deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting classroom {ClassroomId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    [HttpPost("{classroomId}/children/{childId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> EnrollChild(Guid classroomId, Guid childId, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new EnrollChildCommand(classroomId, childId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<object>.Success(null!, "Child enrolled successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enrolling child");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }

    [HttpDelete("{classroomId}/children/{childId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveChild(Guid classroomId, Guid childId, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new RemoveChildCommand(classroomId, childId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            return result.IsFailure
                ? BadRequest(ApiResponse<object>.Fail(result.Error))
                : Ok(ApiResponse<object>.Success(null!, "Child removed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing child");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred"));
        }
    }
}

public class CreateClassroomRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [Range(0, 84)]
    public int MinAgeMonths { get; set; }

    [Range(0, 84)]
    public int MaxAgeMonths { get; set; }

    [Range(1, 30)]
    public int MaxCapacity { get; set; }

    public Guid? LeadTeacherId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}

public class UpdateClassroomRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public required string Name { get; set; }

    [Range(0, 84)]
    public int MinAgeMonths { get; set; }

    [Range(0, 84)]
    public int MaxAgeMonths { get; set; }

    [Range(1, 30)]
    public int MaxCapacity { get; set; }

    [Required]
    public required string Status { get; set; }

    public Guid? LeadTeacherId { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }
}
