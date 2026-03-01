using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Backend.Domain;
using NSubstitute;

namespace Backend.Application.Tests;

public sealed class IncomePeriodServiceTests
{
    [Fact]
    public async Task CreateAsync_WithInvalidDateRange_Throws()
    {
        var gateway = Substitute.For<IIncomePeriodGateway>();
        var sut = new IncomePeriodService(gateway);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.CreateAsync(Guid.NewGuid(), new UpsertIncomePeriodRequest(
                new DateOnly(2026, 3, 10),
                new DateOnly(2026, 3, 9),
                0m), CancellationToken.None));

        await gateway.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExistingPeriod()
    {
        var gateway = Substitute.For<IIncomePeriodGateway>();
        var sut = new IncomePeriodService(gateway);

        var userId = Guid.NewGuid();
        var id = Guid.NewGuid();
        var existing = new IncomePeriod
        {
            Id = id,
            UserId = userId,
            PeriodStartDate = new DateOnly(2026, 2, 1),
            PeriodEndDate = new DateOnly(2026, 2, 28),
            StartingBalance = 100m,
            CreatedAt = DateTime.UtcNow
        };

        gateway.GetByIdAsync(userId, id, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await sut.UpdateAsync(userId, id, new UpsertIncomePeriodRequest(
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            500m), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(new DateOnly(2026, 3, 1), existing.PeriodStartDate);
        Assert.Equal(new DateOnly(2026, 3, 31), existing.PeriodEndDate);
        Assert.Equal(500m, existing.StartingBalance);
        await gateway.Received(1).UpdateAsync(existing, Arg.Any<CancellationToken>());
    }
}
