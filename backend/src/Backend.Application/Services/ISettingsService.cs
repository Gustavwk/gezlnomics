using Backend.Application.Models;

namespace Backend.Application.Services;

public interface ISettingsService
{
    Task<UserSettingsDto> GetAsync(Guid userId, CancellationToken cancellationToken);
    Task<UserSettingsDto> UpdateAsync(Guid userId, UpdateSettingsRequest request, CancellationToken cancellationToken);
}
