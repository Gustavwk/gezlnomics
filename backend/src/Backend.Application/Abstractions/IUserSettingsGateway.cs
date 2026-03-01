using Backend.Domain;

namespace Backend.Application.Abstractions;

public interface IUserSettingsGateway
{
    Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task UpsertAsync(UserSettings settings, CancellationToken cancellationToken);
}
