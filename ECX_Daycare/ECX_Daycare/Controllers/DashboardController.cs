using MediatR;
using Microsoft.AspNetCore.Mvc;
using Daycare.Application.DTOs;
using Daycare.Application.Features.Dashboard.Queries;
using Daycare.Domain.Results;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Dashboard API controller providing role-based overview endpoints.
/// </summary>
[ApiController]
[Route("api/dashboards")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the admin dashboard overview.
    /// </summary>
    [HttpGet("admin/overview")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAdminOverview(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminDashboardQuery(), cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Admin dashboard query failed: {Error}", result.ErrorMessage);
            return BadRequest(new { error = result.ErrorMessage });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets the attendance trend for the last N days (admin).
    /// </summary>
    [HttpGet("admin/attendance/trend")]
    [Produces("application/json")]
    public async Task<IActionResult> GetAttendanceTrend([FromQuery] int days = 7, CancellationToken cancellationToken = default)
    {
        var trend = GenerateMockAttendanceTrend(days);
        return Ok(trend);
    }

    /// <summary>
    /// Gets classroom utilization metrics (admin).
    /// </summary>
    [HttpGet("admin/classrooms/utilization")]
    [Produces("application/json")]
    public async Task<IActionResult> GetClassroomUtilization(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminDashboardQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        var utilization = result.Value.ChildrenByClassroom
            .Select(c => new
            {
                classroomId = c.ClassroomId,
                classroomName = c.ClassroomName,
                capacity = 20,
                enrolled = c.ChildCount,
                present = 0,
                utilization = c.ChildCount,
                occupancyRate = c.ChildCount
            })
            .ToList();

        return Ok(utilization);
    }

    /// <summary>
    /// Gets recent incidents (admin).
    /// </summary>
    [HttpGet("admin/incidents/recent")]
    [Produces("application/json")]
    public async Task<IActionResult> GetRecentIncidents([FromQuery] int limit = 10, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAdminDashboardQuery(), cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        var incidents = result.Value.RecentIncidents
            .Take(limit)
            .Select(i => new
            {
                id = i.Id,
                childId = i.ChildId,
                childName = i.ChildName,
                classroomId = (Guid?)null,
                classroomName = (string?)null,
                type = i.InjuryType.ToString().ToLower(),
                description = i.Description,
                severity = "medium",
                reportedAt = i.IncidentTime,
                reportedBy = "System",
                status = "open"
            })
            .ToList();

        return Ok(incidents);
    }

    /// <summary>
    /// Generates mock attendance trend data for the given number of days.
    /// </summary>
    private static List<object> GenerateMockAttendanceTrend(int days)
    {
        var trend = new List<object>();
        for (int i = days - 1; i >= 0; i--)
        {
            var date = DateTime.UtcNow.Date.AddDays(-i);
            var total = Random.Shared.Next(60, 100);
            var present = (int)(total * (0.8 + Random.Shared.NextDouble() * 0.15));
            trend.Add(new
            {
                date = date.ToString("yyyy-MM-dd"),
                presentCount = present,
                absentCount = total - present,
                totalCount = total,
                rate = (int)Math.Round((double)present / total * 100)
            });
        }
        return trend;
    }
}