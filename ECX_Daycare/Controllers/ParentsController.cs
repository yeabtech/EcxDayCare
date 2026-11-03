using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ECX_Daycare.Common;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Parents.Commands;
using Daycare.Application.Features.Parents.Queries;

namespace ECX_Daycare.Controllers;

/// <summary>
/// API controller for Parent management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireAdmin")]
public class ParentsController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly ILogger<ParentsController> _logger;

    public ParentsController(IMessageBus messageBus, ILogger<ParentsController> logger)
    {
        _messageBus = messageBus;
        _logger = logger;
    }

    /// <summary>
    /// Get all parents with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ParentListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            var query = new GetAllParentsQuery(page, pageSize);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<ParentListResponse>.Success(result.Value, "Parents retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parents");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while retrieving parents"));
        }
    }

    /// <summary>
    /// Search parents by name or email
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<ParentListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest(ApiResponse<object>.Fail("Search term is required"));

            var query = new SearchParentsQuery(q.Trim(), page, pageSize);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<ParentListResponse>.Success(result.Value, "Search completed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching parents");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while searching parents"));
        }
    }

    /// <summary>
    /// Get a single parent by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ParentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetParentQuery(id);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return NotFound(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<ParentDto>.Success(result.Value, "Parent retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parent {ParentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while retrieving parent"));
        }
    }

    /// <summary>
    /// Create a new parent
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ParentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateParentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request data"));

            var command = new CreateParentCommand(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Address,
                request.Notes);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, ApiResponse<ParentDto>.Success(result.Value, "Parent created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating parent");
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while creating parent"));
        }
    }

    /// <summary>
    /// Update an existing parent
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateParentRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.Fail("Invalid request data"));

            var command = new UpdateParentCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Address,
                request.Status,
                request.Notes);

            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Success(null!, "Parent updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating parent {ParentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while updating parent"));
        }
    }

    /// <summary>
    /// Delete a parent
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new DeleteParentCommand(id);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return NotFound(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Success(null!, "Parent deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting parent {ParentId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while deleting parent"));
        }
    }

    /// <summary>
    /// Link a child to a parent
    /// </summary>
    [HttpPost("{parentId}/children/{childId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LinkChild(Guid parentId, Guid childId, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new LinkChildToParentCommand(parentId, childId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Success(null!, "Child linked to parent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error linking child {ChildId} to parent {ParentId}", childId, parentId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while linking child"));
        }
    }

    /// <summary>
    /// Remove a child from a parent
    /// </summary>
    [HttpDelete("{parentId}/children/{childId}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UnlinkChild(Guid parentId, Guid childId, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = new UnlinkChildFromParentCommand(parentId, childId);
            var result = await _messageBus.SendAsync(command, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<object>.Success(null!, "Child removed from parent successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlinking child {ChildId} from parent {ParentId}", childId, parentId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while unlinking child"));
        }
    }

    /// <summary>
    /// Get all parents for a specific child
    /// </summary>
    [HttpGet("child/{childId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ParentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByChild(Guid childId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetParentsByChildQuery(childId);
            var result = await _messageBus.SendAsync(query, cancellationToken);

            if (result.IsFailure)
                return BadRequest(ApiResponse<object>.Fail(result.Error));

            return Ok(ApiResponse<IEnumerable<ParentDto>>.Success(result.Value, "Parents retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving parents for child {ChildId}", childId);
            return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<object>.Fail("An error occurred while retrieving parents"));
        }
    }
}

/// <summary>
/// Request model for creating a parent
/// </summary>
public class CreateParentRequest
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public required string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 500 characters")]
    public required string Address { get; set; }

    [StringLength(1000, ErrorMessage = "Notes must not exceed 1000 characters")]
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for updating a parent
/// </summary>
public class UpdateParentRequest
{
    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 100 characters")]
    public required string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 100 characters")]
    public required string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    public required string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public required string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Address must be between 5 and 500 characters")]
    public required string Address { get; set; }

    [Required(ErrorMessage = "Status is required")]
    public required string Status { get; set; }

    [StringLength(1000, ErrorMessage = "Notes must not exceed 1000 characters")]
    public string? Notes { get; set; }
}
