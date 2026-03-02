using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Backend.Domain;

namespace Backend.Api.IntegrationTests;

public sealed class AccountEndpointsTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AccountEndpointsTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DeleteAccount_RemovesOwnData_AndKeepsOtherUsersData()
    {
        _factory.Store.Users.Clear();
        _factory.Store.Settings.Clear();
        _factory.Store.IncomePeriods.Clear();
        _factory.Store.Transactions.Clear();
        _factory.Store.RecurringRules.Clear();

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        SeedUserGraph(userA, "a@test.local");
        SeedUserGraph(userB, "b@test.local");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userA.ToString());

        var deleteResponse = await client.DeleteAsync("/api/account/");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        Assert.DoesNotContain(_factory.Store.Users, x => x.Id == userA);
        Assert.DoesNotContain(_factory.Store.Settings, x => x.UserId == userA);
        Assert.DoesNotContain(_factory.Store.IncomePeriods, x => x.UserId == userA);
        Assert.DoesNotContain(_factory.Store.Transactions, x => x.UserId == userA);
        Assert.DoesNotContain(_factory.Store.RecurringRules, x => x.UserId == userA);

        Assert.Contains(_factory.Store.Users, x => x.Id == userB);
        Assert.Contains(_factory.Store.Settings, x => x.UserId == userB);
        Assert.Contains(_factory.Store.IncomePeriods, x => x.UserId == userB);
        Assert.Contains(_factory.Store.Transactions, x => x.UserId == userB);
        Assert.Contains(_factory.Store.RecurringRules, x => x.UserId == userB);
    }

    [Fact]
    public async Task Export_ReturnsOnlyOwnData_WithExpectedSchema()
    {
        _factory.Store.Users.Clear();
        _factory.Store.Settings.Clear();
        _factory.Store.IncomePeriods.Clear();
        _factory.Store.Transactions.Clear();
        _factory.Store.RecurringRules.Clear();

        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        SeedUserGraph(userA, "a@test.local");
        SeedUserGraph(userB, "b@test.local");

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Test-UserId", userA.ToString());

        var response = await client.GetAsync("/api/account/export");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("settings", out var settings));
        Assert.True(root.TryGetProperty("incomePeriods", out var periods));
        Assert.True(root.TryGetProperty("transactions", out var transactions));
        Assert.True(root.TryGetProperty("recurringRules", out var recurring));

        Assert.Equal("DKK", settings.GetProperty("currencyCode").GetString());
        Assert.Single(periods.EnumerateArray());
        Assert.Single(transactions.EnumerateArray());
        Assert.Single(recurring.EnumerateArray());

        var txCategory = transactions.EnumerateArray().Single().GetProperty("category").GetString();
        Assert.Equal("A Category", txCategory);

        var recurringTitle = recurring.EnumerateArray().Single().GetProperty("title").GetString();
        Assert.Equal("A Rule", recurringTitle);
    }

    private void SeedUserGraph(Guid userId, string usernamePrefix)
    {
        var now = DateTime.UtcNow;
        var isA = usernamePrefix.StartsWith("a", StringComparison.OrdinalIgnoreCase);
        _factory.Store.Users.Add(new User { Id = userId, Username = usernamePrefix, PasswordHash = "hash", CreatedAt = now, UpdatedAt = now });
        _factory.Store.Settings.Add(new UserSettings
        {
            UserId = userId,
            PaydayDayOfMonth = 1,
            CurrencyCode = "DKK",
            Timezone = "Europe/Copenhagen",
            UpdatedAt = now
        });
        _factory.Store.IncomePeriods.Add(new IncomePeriod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PeriodStartDate = new DateOnly(2026, 3, 1),
            PeriodEndDate = new DateOnly(2026, 3, 31),
            StartingBalance = 1000m,
            CreatedAt = now
        });
        _factory.Store.Transactions.Add(new LedgerTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = new DateOnly(2026, 3, 2),
            Amount = 100m,
            Category = isA ? "A Category" : "B Category",
            Note = "note",
            Kind = TransactionKind.ExpenseActual,
            Status = TransactionStatus.Active,
            CreatedAt = now,
            UpdatedAt = now
        });
        _factory.Store.RecurringRules.Add(new RecurringRule
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = isA ? "A Rule" : "B Rule",
            Amount = 50m,
            Category = "Fixed",
            Note = "note",
            RuleKind = TransactionKind.ExpensePlanned,
            Frequency = RecurringFrequency.Monthly,
            StartDate = new DateOnly(2026, 3, 1),
            EndDate = null,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });
    }
}
