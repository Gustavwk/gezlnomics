using Backend.Application.Abstractions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Backend.Api.IntegrationTests;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    internal InMemoryDataStore Store { get; } = new();

    public TestWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Default", "Host=localhost;Port=5432;Database=test;Username=test;Password=test");
        Environment.SetEnvironmentVariable("Frontend__Origin", "http://localhost:3000");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserGateway>();
            services.RemoveAll<IUserSettingsGateway>();
            services.RemoveAll<IIncomePeriodGateway>();
            services.RemoveAll<ITransactionGateway>();
            services.RemoveAll<IRecurringRuleGateway>();
            services.RemoveAll<IHostedService>();

            services.AddSingleton(Store);
            services.AddSingleton<IUserGateway, InMemoryUserGateway>();
            services.AddSingleton<IUserSettingsGateway, InMemoryUserSettingsGateway>();
            services.AddSingleton<IIncomePeriodGateway, InMemoryIncomePeriodGateway>();
            services.AddSingleton<ITransactionGateway, InMemoryTransactionGateway>();
            services.AddSingleton<IRecurringRuleGateway, InMemoryRecurringRuleGateway>();

            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });
            services.PostConfigure<AuthenticationOptions>(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            });
        });
    }
}
