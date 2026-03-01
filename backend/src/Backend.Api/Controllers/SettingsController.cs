using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/settings")]
public sealed class SettingsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] ICurrentUserService currentUserService, [FromServices] ISettingsService settingsService, CancellationToken cancellationToken)
    {
        var settings = await settingsService.GetAsync(currentUserService.GetRequiredUserId(), cancellationToken);
        return Ok(settings);
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest request, [FromServices] ICurrentUserService currentUserService, [FromServices] ISettingsService settingsService, CancellationToken cancellationToken)
    {
        try
        {
            var settings = await settingsService.UpdateAsync(currentUserService.GetRequiredUserId(), request, cancellationToken);
            return Ok(settings);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
