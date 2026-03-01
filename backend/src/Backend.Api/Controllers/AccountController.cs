using Backend.Application.Abstractions;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public sealed class AccountController : ControllerBase
{
    [HttpGet("export")]
    public async Task<IActionResult> Export([FromServices] ICurrentUserService currentUserService, [FromServices] IAccountService service, CancellationToken cancellationToken)
    {
        var export = await service.ExportAsync(currentUserService.GetRequiredUserId(), cancellationToken);
        return export is null ? NotFound() : Ok(export);
    }

    [HttpDelete]
    public async Task<IActionResult> Delete([FromServices] ICurrentUserService currentUserService, [FromServices] IAccountService service, CancellationToken cancellationToken)
    {
        await service.DeleteAccountAsync(currentUserService.GetRequiredUserId(), cancellationToken);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
