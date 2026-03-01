using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/recurring-rules")]
public sealed class RecurringRulesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromServices] ICurrentUserService currentUserService, [FromServices] IRecurringRuleService service, CancellationToken cancellationToken)
    {
        var rules = await service.GetAllAsync(currentUserService.GetRequiredUserId(), cancellationToken);
        return Ok(rules);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] UpsertRecurringRuleRequest request, [FromServices] ICurrentUserService currentUserService, [FromServices] IRecurringRuleService service, CancellationToken cancellationToken)
    {
        try
        {
            var rule = await service.CreateAsync(currentUserService.GetRequiredUserId(), request, cancellationToken);
            return Ok(rule);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpsertRecurringRuleRequest request, [FromServices] ICurrentUserService currentUserService, [FromServices] IRecurringRuleService service, CancellationToken cancellationToken)
    {
        try
        {
            var rule = await service.UpdateAsync(currentUserService.GetRequiredUserId(), id, request, cancellationToken);
            return rule is null ? NotFound() : Ok(rule);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] ICurrentUserService currentUserService, [FromServices] IRecurringRuleService service, CancellationToken cancellationToken)
    {
        var deleted = await service.DeleteAsync(currentUserService.GetRequiredUserId(), id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
