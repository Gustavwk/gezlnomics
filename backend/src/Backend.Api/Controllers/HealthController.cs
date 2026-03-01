using Backend.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Route("")]
public sealed class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult Health() => Ok("OK");

    [HttpGet("health/live")]
    public IActionResult Live() => Ok(new { status = "ok" });

    [HttpGet("health/ready")]
    public async Task<IActionResult> Ready([FromServices] AppDbContext dbContext, CancellationToken cancellationToken)
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
        return canConnect
            ? Ok(new { status = "ready" })
            : StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
}
