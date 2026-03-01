namespace Backend.Application.Models;

public sealed record UserSettingsDto(int PaydayDayOfMonth, string CurrencyCode, string Timezone);
public sealed record UpdateSettingsRequest(int PaydayDayOfMonth, string CurrencyCode, string Timezone);
