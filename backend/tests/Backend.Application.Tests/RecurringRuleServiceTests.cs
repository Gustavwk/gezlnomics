using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Backend.Domain;
using NSubstitute;

namespace Backend.Application.Tests;

public sealed class RecurringRuleServiceTests
{
    [Fact]
    public async Task CreateAsync_NormalizesValues()
    {
        var gateway = Substitute.For<IRecurringRuleGateway>();
        var sut = new RecurringRuleService(gateway);

        var dto = await sut.CreateAsync(Guid.NewGuid(), new UpsertRecurringRuleRequest(
            "  Husleje  ",
            -5000m,
            "   ",
            "  fast  ",
            TransactionKind.ExpensePlanned,
            RecurringFrequency.Monthly,
            new DateOnly(2026, 3, 1),
            null,
            true), CancellationToken.None);

        Assert.Equal("Husleje", dto.Title);
        Assert.Equal(5000m, dto.Amount);
        Assert.Equal("General", dto.Category);
        Assert.Equal("fast", dto.Note);

        await gateway.Received(1).AddAsync(
            Arg.Is<RecurringRule>(r => r.Title == "Husleje" && r.Amount == 5000m && r.Category == "General" && r.Note == "fast"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_WithInvalidDateRange_Throws()
    {
        var gateway = Substitute.For<IRecurringRuleGateway>();
        var sut = new RecurringRuleService(gateway);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateAsync(Guid.NewGuid(), new UpsertRecurringRuleRequest(
                "A", 1m, "Cat", null,
                TransactionKind.ExpensePlanned,
                RecurringFrequency.Monthly,
                new DateOnly(2026, 3, 2),
                new DateOnly(2026, 3, 1),
                true), CancellationToken.None));

        await gateway.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task UpdateAsync_WhenMissingRule_ReturnsNull()
    {
        var gateway = Substitute.For<IRecurringRuleGateway>();
        var sut = new RecurringRuleService(gateway);

        gateway.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((RecurringRule?)null);

        var result = await sut.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), new UpsertRecurringRuleRequest(
            "Titel", 100m, "Cat", null,
            TransactionKind.ExpensePlanned,
            RecurringFrequency.Monthly,
            new DateOnly(2026, 3, 1),
            null,
            true), CancellationToken.None);

        Assert.Null(result);
    }
}
