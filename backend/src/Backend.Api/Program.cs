using System.Security.Claims;
using Backend.Api;
using Backend.Application;
using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Backend.Domain;
using Backend.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Backend.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth_token";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.None
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var frontendOrigin = builder.Configuration["Frontend:Origin"];
        if (!string.IsNullOrWhiteSpace(frontendOrigin))
        {
            policy.WithOrigins(frontendOrigin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else
        {
            throw new InvalidOperationException("Frontend:Origin skal være sat i production.");
        }
    });
});

new InfrastructureRegistrar().Register(builder.Services, builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok("OK"));
app.MapGet("/health/live", () => Results.Ok(new { status = "ok" }));
app.MapGet("/health/ready", async (AppDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
    return canConnect
        ? Results.Ok(new { status = "ready" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});
app.MapGet("/api/ping", (IPingService pingService) => Results.Ok(pingService.GetPing()));

app.MapPost("/api/auth/signup", async (SignupRequest request, IAuthService authService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    try
    {
        var user = await authService.SignupAsync(request, cancellationToken);
        await SignInAsync(httpContext, user);
        return Results.Ok(user);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.MapPost("/api/auth/login", async (LoginRequest request, IAuthService authService, HttpContext httpContext, CancellationToken cancellationToken) =>
{
    var user = await authService.LoginAsync(request, cancellationToken);
    if (user is null)
    {
        return Results.Unauthorized();
    }

    await SignInAsync(httpContext, user);
    return Results.Ok(user);
});

app.MapPost("/api/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Ok();
});

app.MapGet("/api/auth/me", async (ICurrentUserService currentUserService, IAuthService authService, CancellationToken cancellationToken) =>
{
    var userId = currentUserService.GetUserId();
    if (!userId.HasValue)
    {
        return Results.Unauthorized();
    }

    var user = await authService.GetCurrentUserAsync(userId.Value, cancellationToken);
    return user is null ? Results.Unauthorized() : Results.Ok(user);
}).RequireAuthorization();

var settingsGroup = app.MapGroup("/api/settings").RequireAuthorization();
settingsGroup.MapGet("/", async (ICurrentUserService currentUserService, ISettingsService settingsService, CancellationToken cancellationToken) =>
{
    var settings = await settingsService.GetAsync(currentUserService.GetRequiredUserId(), cancellationToken);
    return Results.Ok(settings);
});
settingsGroup.MapPut("/", async (UpdateSettingsRequest request, ICurrentUserService currentUserService, ISettingsService settingsService, CancellationToken cancellationToken) =>
{
    try
    {
        var settings = await settingsService.UpdateAsync(currentUserService.GetRequiredUserId(), request, cancellationToken);
        return Results.Ok(settings);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

var incomeGroup = app.MapGroup("/api/income-periods").RequireAuthorization();
incomeGroup.MapGet("/", async (DateOnly? from, DateOnly? to, ICurrentUserService currentUserService, IIncomePeriodService service, CancellationToken cancellationToken) =>
{
    var periods = await service.GetAllAsync(currentUserService.GetRequiredUserId(), from, to, cancellationToken);
    return Results.Ok(periods);
});
incomeGroup.MapPost("/", async (UpsertIncomePeriodRequest request, ICurrentUserService currentUserService, IIncomePeriodService service, CancellationToken cancellationToken) =>
{
    try
    {
        var period = await service.CreateAsync(currentUserService.GetRequiredUserId(), request, cancellationToken);
        return Results.Ok(period);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});
incomeGroup.MapPut("/{id:guid}", async (Guid id, UpsertIncomePeriodRequest request, ICurrentUserService currentUserService, IIncomePeriodService service, CancellationToken cancellationToken) =>
{
    try
    {
        var period = await service.UpdateAsync(currentUserService.GetRequiredUserId(), id, request, cancellationToken);
        return period is null ? Results.NotFound() : Results.Ok(period);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});
incomeGroup.MapDelete("/{id:guid}", async (Guid id, ICurrentUserService currentUserService, IIncomePeriodService service, CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(currentUserService.GetRequiredUserId(), id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

var txGroup = app.MapGroup("/api/transactions").RequireAuthorization();
txGroup.MapGet("/", async (DateOnly? from, DateOnly? to, TransactionKind? kind, string? category, string? q, ICurrentUserService currentUserService, ITransactionService service, CancellationToken cancellationToken) =>
{
    var transactions = await service.GetAllAsync(currentUserService.GetRequiredUserId(), from, to, kind, category, q, cancellationToken);
    return Results.Ok(transactions);
});
txGroup.MapPost("/", async (UpsertTransactionRequest request, ICurrentUserService currentUserService, ITransactionService service, CancellationToken cancellationToken) =>
{
    var transaction = await service.CreateAsync(currentUserService.GetRequiredUserId(), request, cancellationToken);
    return Results.Ok(transaction);
});
txGroup.MapPut("/{id:guid}", async (Guid id, UpsertTransactionRequest request, ICurrentUserService currentUserService, ITransactionService service, CancellationToken cancellationToken) =>
{
    var transaction = await service.UpdateAsync(currentUserService.GetRequiredUserId(), id, request, cancellationToken);
    return transaction is null ? Results.NotFound() : Results.Ok(transaction);
});
txGroup.MapDelete("/{id:guid}", async (Guid id, ICurrentUserService currentUserService, ITransactionService service, CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(currentUserService.GetRequiredUserId(), id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

var recurringGroup = app.MapGroup("/api/recurring-rules").RequireAuthorization();
recurringGroup.MapGet("/", async (ICurrentUserService currentUserService, IRecurringRuleService service, CancellationToken cancellationToken) =>
{
    var rules = await service.GetAllAsync(currentUserService.GetRequiredUserId(), cancellationToken);
    return Results.Ok(rules);
});
recurringGroup.MapPost("/", async (UpsertRecurringRuleRequest request, ICurrentUserService currentUserService, IRecurringRuleService service, CancellationToken cancellationToken) =>
{
    try
    {
        var rule = await service.CreateAsync(currentUserService.GetRequiredUserId(), request, cancellationToken);
        return Results.Ok(rule);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});
recurringGroup.MapPut("/{id:guid}", async (Guid id, UpsertRecurringRuleRequest request, ICurrentUserService currentUserService, IRecurringRuleService service, CancellationToken cancellationToken) =>
{
    try
    {
        var rule = await service.UpdateAsync(currentUserService.GetRequiredUserId(), id, request, cancellationToken);
        return rule is null ? Results.NotFound() : Results.Ok(rule);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});
recurringGroup.MapDelete("/{id:guid}", async (Guid id, ICurrentUserService currentUserService, IRecurringRuleService service, CancellationToken cancellationToken) =>
{
    var deleted = await service.DeleteAsync(currentUserService.GetRequiredUserId(), id, cancellationToken);
    return deleted ? Results.NoContent() : Results.NotFound();
});

var ledgerGroup = app.MapGroup("/api/ledger").RequireAuthorization();
ledgerGroup.MapGet("/summary", async (DateOnly? asOf, ICurrentUserService currentUserService, ILedgerService service, CancellationToken cancellationToken) =>
{
    var summary = await service.GetSummaryAsync(currentUserService.GetRequiredUserId(), asOf ?? DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);
    return Results.Ok(summary);
});
ledgerGroup.MapGet("/timeline", async (DateOnly from, DateOnly to, ICurrentUserService currentUserService, ILedgerService service, CancellationToken cancellationToken) =>
{
    try
    {
        var timeline = await service.GetTimelineAsync(currentUserService.GetRequiredUserId(), from, to, cancellationToken);
        return Results.Ok(timeline);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

app.Run();

static async Task SignInAsync(HttpContext httpContext, AuthUserDto user)
{
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email)
    };

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
}
