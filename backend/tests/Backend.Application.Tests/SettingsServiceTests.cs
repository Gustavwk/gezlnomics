using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Backend.Domain;
using NSubstitute;

namespace Backend.Application.Tests;

public sealed class SettingsServiceTests
{
    [Fact]
    public async Task UpdateAsync_WhenInvalidPayday_Throws()
    {
        var gateway = Substitute.For<IUserSettingsGateway>();
        var sut = new SettingsService(gateway);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.UpdateAsync(Guid.NewGuid(), new UpdateSettingsRequest(0, "DKK", "Europe/Copenhagen"), CancellationToken.None));
    }

    [Fact]
    public async Task GetAsync_WhenMissingSettings_CreatesDefaults()
    {
        var gateway = Substitute.For<IUserSettingsGateway>();
        var sut = new SettingsService(gateway);
        var userId = Guid.NewGuid();

        gateway.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((UserSettings?)null);

        var result = await sut.GetAsync(userId, CancellationToken.None);

        Assert.Equal(1, result.PaydayDayOfMonth);
        Assert.Equal("DKK", result.CurrencyCode);
        Assert.Equal("Europe/Copenhagen", result.Timezone);

        await gateway.Received(1).UpsertAsync(
            Arg.Is<UserSettings>(s => s.UserId == userId && s.PaydayDayOfMonth == 1 && s.CurrencyCode == "DKK"),
            Arg.Any<CancellationToken>());
    }
}
