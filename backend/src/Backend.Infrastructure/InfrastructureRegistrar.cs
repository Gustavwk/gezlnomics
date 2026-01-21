using Backend.Application.Abstractions;
using Backend.Infrastructure.Data;
using Backend.Infrastructure.Gateways;
using Backend.Infrastructure.HostedServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Infrastructure;

public sealed class InfrastructureRegistrar : IInfrastructureRegistrar
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Default' was not configured.");
        }

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IExpenseGateway, ExpenseGateway>();
        services.AddScoped<IStartingCapitalGateway, StartingCapitalGateway>();

        services.AddHostedService<MigrationHostedService>();
    }
}
