using System.Reflection;
using System.Text.Json.Serialization;
using Backend.Application;
using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

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

app.MapPut("/api/users/{userId:guid}/months/{year:int}/{month:int}/cashflow", async (
    Guid userId,
    int year,
    int month,
    UpsertUserMonthCashflowRequest request,
    ICashflowForecastService cashflowForecastService,
    CancellationToken cancellationToken) =>
{
    await cashflowForecastService.SaveUserMonthAsync(userId, year, month, request, cancellationToken);
    return Results.NoContent();
});

app.MapPost("/api/users/{userId:guid}/months/{year:int}/{month:int}/forecast", async (
    Guid userId,
    int year,
    int month,
    CashflowForecastRequest request,
    ICashflowForecastService cashflowForecastService,
    CancellationToken cancellationToken) =>
{
    var result = await cashflowForecastService.GetForecastAsync(userId, year, month, request, cancellationToken);
    return result is null ? Results.NotFound() : Results.Ok(result);
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
