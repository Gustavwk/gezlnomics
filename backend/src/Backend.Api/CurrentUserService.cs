using System.Security.Claims;
using Backend.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Backend.Api;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetRequiredUserId()
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            throw new InvalidOperationException("User is not authenticated.");
        }

        return userId.Value;
    }

    public Guid? GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        var raw = principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal?.FindFirstValue("sub");
        return Guid.TryParse(raw, out var userId) ? userId : null;
    }
}
