using System.Reflection;
using Backend.Application;
using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var frontendOrigin = builder.Configuration["Frontend:Origin"];
        if (!string.IsNullOrWhiteSpace(frontendOrigin))
        {
            policy.WithOrigins(frontendOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

InfrastructureLoader.Register(builder.Services, builder.Configuration);

var app = builder.Build();

app.UseCors("Frontend");

app.MapGet("/health", () => Results.Ok("OK"));

app.MapGet("/api/ping", (IPingService pingService) => Results.Ok(pingService.GetPing()));

app.MapGet("/api/expenses", async (IExpenseService expenseService, CancellationToken cancellationToken) =>
{
    var expenses = await expenseService.GetExpensesAsync(cancellationToken);
    return Results.Ok(expenses);
});

app.MapPost("/api/expenses", async (CreateExpenseRequest request, IExpenseService expenseService, CancellationToken cancellationToken) =>
{
    var expense = await expenseService.AddExpenseAsync(request, cancellationToken);
    return Results.Ok(expense);
});

app.Run();

internal static class InfrastructureLoader
{
    public static void Register(IServiceCollection services, IConfiguration configuration)
    {
        var assemblyName = "Backend.Infrastructure";
        var assemblyPath = Path.Combine(AppContext.BaseDirectory, $"{assemblyName}.dll");
        var assembly = File.Exists(assemblyPath)
            ? Assembly.LoadFrom(assemblyPath)
            : Assembly.Load(assemblyName);

        var registrarType = assembly.GetTypes()
            .FirstOrDefault(type => typeof(IInfrastructureRegistrar).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

        if (registrarType is null)
        {
            throw new InvalidOperationException("Infrastructure registrar was not found.");
        }

        if (Activator.CreateInstance(registrarType) is not IInfrastructureRegistrar registrar)
        {
            throw new InvalidOperationException("Infrastructure registrar could not be instantiated.");
        }

        registrar.Register(services, configuration);
    }
}
