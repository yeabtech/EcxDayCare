using Microsoft.AspNetCore.Mvc;

namespace ECX_Daycare.Controllers;

/// <summary>
/// Health check endpoint for the API.
/// Used for monitoring and load balancer health checks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Gets the current health status of the API.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<object> GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
