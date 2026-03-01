using Backend.Application.Abstractions;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ledger")]
public sealed class LedgerController : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] DateOnly? asOf, [FromServices] ICurrentUserService currentUserService, [FromServices] ILedgerService service, CancellationToken cancellationToken)
    {
        var summary = await service.GetSummaryAsync(currentUserService.GetRequiredUserId(), asOf ?? DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
        return Ok(summary);
    }

    [HttpGet("timeline")]
    public async Task<IActionResult> Timeline([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromServices] ICurrentUserService currentUserService, [FromServices] ILedgerService service, CancellationToken cancellationToken)
    {
        try
        {
            var timeline = await service.GetTimelineAsync(currentUserService.GetRequiredUserId(), from, to, cancellationToken);
            return Ok(timeline);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
