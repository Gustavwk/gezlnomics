using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/income-periods")]
public sealed class IncomePeriodsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] DateOnly? from, [FromQuery] DateOnly? to, [FromServices] ICurrentUserService currentUserService, [FromServices] IIncomePeriodService service, CancellationToken cancellationToken)
    {
        var periods = await service.GetAllAsync(currentUserService.GetRequiredUserId(), from, to, cancellationToken);
        return Ok(periods);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertIncomePeriodRequest request, [FromServices] ICurrentUserService currentUserService, [FromServices] IIncomePeriodService service, CancellationToken cancellationToken)
    {
        try
        {
            var period = await service.CreateAsync(currentUserService.GetRequiredUserId(), request, cancellationToken);
            return Ok(period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertIncomePeriodRequest request, [FromServices] ICurrentUserService currentUserService, [FromServices] IIncomePeriodService service, CancellationToken cancellationToken)
    {
        try
        {
            var period = await service.UpdateAsync(currentUserService.GetRequiredUserId(), id, request, cancellationToken);
            return period is null ? NotFound() : Ok(period);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] ICurrentUserService currentUserService, [FromServices] IIncomePeriodService service, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(currentUserService.GetRequiredUserId(), id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
