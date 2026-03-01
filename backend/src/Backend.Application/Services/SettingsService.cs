using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class SettingsService : ISettingsService
{
    private readonly IUserSettingsGateway _settingsGateway;

    public SettingsService(IUserSettingsGateway settingsGateway)
    {
        _settingsGateway = settingsGateway;
    }

    public async Task<UserSettingsDto> GetAsync(Guid userId, CancellationToken cancellationToken)
    {
        var settings = await GetOrCreateSettings(userId, cancellationToken);
        return Map(settings);
    }

    public async Task<UserSettingsDto> UpdateAsync(Guid userId, UpdateSettingsRequest request, CancellationToken cancellationToken)
    {
        if (request.PaydayDayOfMonth is < 1 or > 31)
        {
            throw new InvalidOperationException("PaydayDayOfMonth skal være mellem 1 og 31.");
        }

        var settings = await GetOrCreateSettings(userId, cancellationToken);
        settings.PaydayDayOfMonth = request.PaydayDayOfMonth;
        settings.CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "DKK" : request.CurrencyCode.Trim().ToUpperInvariant();
        settings.Timezone = string.IsNullOrWhiteSpace(request.Timezone) ? "Europe/Copenhagen" : request.Timezone.Trim();
        settings.UpdatedAt = DateTime.UtcNow;
        await _settingsGateway.UpsertAsync(settings, cancellationToken);
        return Map(settings);
    }

    private async Task<UserSettings> GetOrCreateSettings(Guid userId, CancellationToken cancellationToken)
    {
        var existing = await _settingsGateway.GetByUserIdAsync(userId, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = new UserSettings
        {
            UserId = userId,
            PaydayDayOfMonth = 1,
            CurrencyCode = "DKK",
            Timezone = "Europe/Copenhagen",
            UpdatedAt = DateTime.UtcNow
        };

        await _settingsGateway.UpsertAsync(created, cancellationToken);
        return created;
    }

    private static UserSettingsDto Map(UserSettings settings) =>
        new(settings.PaydayDayOfMonth, settings.CurrencyCode, settings.Timezone);
}
