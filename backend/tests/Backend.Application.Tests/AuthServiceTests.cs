using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Application.Services;
using Backend.Domain;
using NSubstitute;

namespace Backend.Application.Tests;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task SignupAsync_CreatesUser_AndDefaultSettings()
    {
        var userGateway = Substitute.For<IUserGateway>();
        var settingsGateway = Substitute.For<IUserSettingsGateway>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var sut = new AuthService(userGateway, settingsGateway, passwordHasher);

        userGateway.GetByUsernameAsync("test_user", Arg.Any<CancellationToken>()).Returns((User?)null);
        passwordHasher.HashPassword("password123").Returns("HASH:password123");

        var result = await sut.SignupAsync(new SignupRequest("  TEST_USER  ", "password123"), CancellationToken.None);

        Assert.Equal("test_user", result.Username);
        await userGateway.Received(1).AddAsync(
            Arg.Is<User>(u => u.Username == "test_user" && u.PasswordHash == "HASH:password123"),
            Arg.Any<CancellationToken>());
        await settingsGateway.Received(1).UpsertAsync(
            Arg.Is<UserSettings>(s => s.CurrencyCode == "DKK" && s.PaydayDayOfMonth == 1 && s.Timezone == "Europe/Copenhagen"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignupAsync_WhenUsernameExists_Throws()
    {
        var userGateway = Substitute.For<IUserGateway>();
        var settingsGateway = Substitute.For<IUserSettingsGateway>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var sut = new AuthService(userGateway, settingsGateway, passwordHasher);

        userGateway.GetByUsernameAsync("taken_user", Arg.Any<CancellationToken>())
            .Returns(new User { Id = Guid.NewGuid(), Username = "taken_user", PasswordHash = "hash" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SignupAsync(new SignupRequest("taken_user", "password123"), CancellationToken.None));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsUser()
    {
        var userGateway = Substitute.For<IUserGateway>();
        var settingsGateway = Substitute.For<IUserSettingsGateway>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var sut = new AuthService(userGateway, settingsGateway, passwordHasher);

        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "user_1",
            PasswordHash = "HASH:password123"
        };

        userGateway.GetByUsernameAsync("user_1", Arg.Any<CancellationToken>()).Returns(user);
        passwordHasher.VerifyPassword("password123", "HASH:password123").Returns(true);

        var result = await sut.LoginAsync(new LoginRequest("user_1", "password123"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("user_1", result!.Username);
    }
}
