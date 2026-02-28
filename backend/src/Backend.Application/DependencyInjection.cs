using Backend.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPingService, PingService>();
        services.AddScoped<IExpenseService, ExpenseService>();
        services.AddScoped<ICashflowForecastService, CashflowForecastService>();
        return services;
    }
}
