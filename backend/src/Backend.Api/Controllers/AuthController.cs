using System.Security.Claims;
using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    [EnableRateLimiting("AuthSignup")]
    [HttpPost("signup")]
    public async Task<IActionResult> Signup([FromBody] SignupRequest request, [FromServices] IAuthService authService, CancellationToken cancellationToken)
    {
        try
        {
            var user = await authService.SignupAsync(request, cancellationToken);
            await SignInAsync(user);
            return Ok(user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [EnableRateLimiting("AuthLogin")]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, [FromServices] IAuthService authService, CancellationToken cancellationToken)
    {
        var user = await authService.LoginAsync(request, cancellationToken);
        if (user is null)
        {
            return Unauthorized();
        }

        await SignInAsync(user);
        return Ok(user);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me([FromServices] ICurrentUserService currentUserService, [FromServices] IAuthService authService, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized();
        }

        var user = await authService.GetCurrentUserAsync(userId.Value, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }

    private async Task SignInAsync(AuthUserDto user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
