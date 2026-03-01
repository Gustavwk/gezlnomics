using Backend.Application.Abstractions;
using Backend.Domain;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Gateways;

public sealed class UserSettingsGateway : IUserSettingsGateway
{
    private readonly AppDbContext _dbContext;

    public UserSettingsGateway(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken) =>
        _dbContext.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

    public async Task UpsertAsync(UserSettings settings, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.UserSettings.FirstOrDefaultAsync(x => x.UserId == settings.UserId, cancellationToken);
        if (existing is null)
        {
            _dbContext.UserSettings.Add(settings);
        }
        else
        {
            existing.PaydayDayOfMonth = settings.PaydayDayOfMonth;
            existing.CurrencyCode = settings.CurrencyCode;
            existing.Timezone = settings.Timezone;
            existing.UpdatedAt = settings.UpdatedAt;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
