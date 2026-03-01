using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class AuthService : IAuthService
{
    private readonly IUserGateway _userGateway;
    private readonly IUserSettingsGateway _settingsGateway;
    private readonly IPasswordHasher _passwordHasher;
    public AuthService(
        IUserGateway userGateway,
        IUserSettingsGateway settingsGateway,
        IPasswordHasher passwordHasher)
    {
        _userGateway = userGateway;
        _settingsGateway = settingsGateway;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthUserDto> SignupAsync(SignupRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
        {
            throw new InvalidOperationException("Email og password er ugyldige.");
        }

        var existing = await _userGateway.GetByEmailAsync(email, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException("Email findes allerede.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _userGateway.AddAsync(user, cancellationToken);
        await _settingsGateway.UpsertAsync(new UserSettings
        {
            UserId = user.Id,
            PaydayDayOfMonth = 1,
            CurrencyCode = "DKK",
            Timezone = "Europe/Copenhagen",
            UpdatedAt = now
        }, cancellationToken);

        return new AuthUserDto(user.Id, user.Email);
    }

    public async Task<AuthUserDto?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _userGateway.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return null;
        }

        return new AuthUserDto(user.Id, user.Email);
    }

    public async Task<AuthUserDto?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userGateway.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : new AuthUserDto(user.Id, user.Email);
    }
}
