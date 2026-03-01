using Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPingService, PingService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IIncomePeriodService, IncomePeriodService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IRecurringRuleService, RecurringRuleService>();
        services.AddScoped<ILedgerService, LedgerService>();
        return services;
    }
}
