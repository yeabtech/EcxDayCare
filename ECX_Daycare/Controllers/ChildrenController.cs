using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Daycare.Application.Features.Children.Commands;
using Daycare.Application.Features.Children.Queries;
using Daycare.Application.DTOs;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Controller for managing children in the daycare system.
/// Provides CRUD operations and status management endpoints.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "RequireStaffOrAdmin")]
public sealed class ChildrenController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ChildrenController> _logger;

    public ChildrenController(IMediator mediator, ILogger<ChildrenController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new child.
    /// </summary>
    /// <param name="request">The child creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created child.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChildDto>> CreateChild(
        CreateChildDto request,
        CancellationToken cancellationToken)
    {
        var command = new CreateChildCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.EnrollmentDate,
            request.PhotoUrl,
            request.Allergies,
            request.MedicalNotes,
            request.Parents);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });

        _logger.LogInformation("Child created with ID: {ChildId}", result.Value.Id);
        return CreatedAtAction(nameof(GetChildById), new { childId = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Retrieves a child by ID.
    /// </summary>
    /// <param name="childId">The child ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The child if found.</returns>
    [HttpGet("{childId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChildDto>> GetChildById(
        Guid childId,
        CancellationToken cancellationToken)
    {
        var query = new GetChildByIdQuery(childId.ToString());
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorMessage, code = result.ErrorCode });

        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves all children with pagination.
    /// </summary>
    /// <param name="skip">Number of records to skip (default: 0).</param>
    /// <param name="take">Number of records to return (default: 10, max: 100).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>Paginated list of children.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChildListDto>> GetAllChildren(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllChildrenQuery(skip, take);
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });

        return Ok(result.Value);
    }

    /// <summary>
    /// Retrieves every parent linked to a child, including the stored role.
    /// </summary>
    /// <param name="childId">The child ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The linked parents with their roles.</returns>
    [HttpGet("{childId:guid}/parents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<ParentLinkDto>>> GetChildParents(
        Guid childId,
        CancellationToken cancellationToken)
    {
        var query = new GetChildParentsQuery(childId.ToString());
        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorMessage, code = result.ErrorCode });

        return Ok(result.Value);
    }

    /// <summary>
    /// Updates an existing child.
    /// </summary>
    /// <param name="childId">The child ID.</param>
    /// <param name="request">The child update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated child.</returns>
    [HttpPut("{childId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChildDto>> UpdateChild(
        Guid childId,
        UpdateChildDto request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateChildCommand(
            childId.ToString(),
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Gender,
            request.PhotoUrl,
            request.Allergies,
            request.MedicalNotes,
            request.Parents,
            request.Status,
            request.EnrollmentDate);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "CHILD_NOT_FOUND"
                ? NotFound(new { error = result.ErrorMessage, code = result.ErrorCode })
                : BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        _logger.LogInformation("Child updated with ID: {ChildId}", childId);
        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a child.
    /// </summary>
    /// <param name="childId">The child ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{childId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteChild(
        Guid childId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteChildCommand(childId.ToString());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return NotFound(new { error = result.ErrorMessage });

        _logger.LogInformation("Child deleted with ID: {ChildId}", childId);
        return NoContent();
    }

    /// <summary>
    /// Marks a child as inactive.
    /// </summary>
    /// <param name="childId">The child ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated child.</returns>
    [HttpPost("{childId:guid}/mark-inactive")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChildDto>> MarkChildInactive(
        Guid childId,
        CancellationToken cancellationToken)
    {
        var command = new MarkChildInactiveCommand(childId.ToString());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "CHILD_NOT_FOUND"
                ? NotFound(new { error = result.ErrorMessage, code = result.ErrorCode })
                : BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        _logger.LogInformation("Child marked inactive with ID: {ChildId}", childId);
        return Ok(result.Value);
    }

    /// <summary>
    /// Marks a child as withdrawn.
    /// </summary>
    /// <param name="childId">The child ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated child.</returns>
    [HttpPost("{childId:guid}/mark-withdrawn")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChildDto>> MarkChildWithdrawn(
        Guid childId,
        CancellationToken cancellationToken)
    {
        var command = new MarkChildWithdrawnCommand(childId.ToString());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "CHILD_NOT_FOUND"
                ? NotFound(new { error = result.ErrorMessage, code = result.ErrorCode })
                : BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        _logger.LogInformation("Child marked withdrawn with ID: {ChildId}", childId);
        return Ok(result.Value);
    }

    /// <summary>
    /// Re-enrolls an inactive or withdrawn child.
    /// </summary>
    /// <param name="childId">The child ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated child.</returns>
    [HttpPost("{childId:guid}/reenroll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChildDto>> ReenrollChild(
        Guid childId,
        CancellationToken cancellationToken)
    {
        var command = new ReenrollChildCommand(childId.ToString());
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.ErrorCode == "CHILD_NOT_FOUND"
                ? NotFound(new { error = result.ErrorMessage, code = result.ErrorCode })
                : BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });
        }

        _logger.LogInformation("Child re-enrolled with ID: {ChildId}", childId);
        return Ok(result.Value);
    }
}
