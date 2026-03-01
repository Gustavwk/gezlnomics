using Backend.Application.Models;

namespace Backend.Application.Services;

public interface IAuthService
{
    Task<AuthUserDto> SignupAsync(SignupRequest request, CancellationToken cancellationToken);
    Task<AuthUserDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<AuthUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
