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

        userGateway.GetByEmailAsync("test@example.com", Arg.Any<CancellationToken>()).Returns((User?)null);
        passwordHasher.HashPassword("password123").Returns("HASH:password123");

        var result = await sut.SignupAsync(new SignupRequest("  TEST@Example.com  ", "password123"), CancellationToken.None);

        Assert.Equal("test@example.com", result.Email);
        await userGateway.Received(1).AddAsync(
            Arg.Is<User>(u => u.Email == "test@example.com" && u.PasswordHash == "HASH:password123"),
            Arg.Any<CancellationToken>());
        await settingsGateway.Received(1).UpsertAsync(
            Arg.Is<UserSettings>(s => s.CurrencyCode == "DKK" && s.PaydayDayOfMonth == 1 && s.Timezone == "Europe/Copenhagen"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SignupAsync_WhenEmailExists_Throws()
    {
        var userGateway = Substitute.For<IUserGateway>();
        var settingsGateway = Substitute.For<IUserSettingsGateway>();
        var passwordHasher = Substitute.For<IPasswordHasher>();
        var sut = new AuthService(userGateway, settingsGateway, passwordHasher);

        userGateway.GetByEmailAsync("taken@example.com", Arg.Any<CancellationToken>())
            .Returns(new User { Id = Guid.NewGuid(), Email = "taken@example.com", PasswordHash = "hash" });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SignupAsync(new SignupRequest("taken@example.com", "password123"), CancellationToken.None));
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
            Email = "user@example.com",
            PasswordHash = "HASH:password123"
        };

        userGateway.GetByEmailAsync("user@example.com", Arg.Any<CancellationToken>()).Returns(user);
        passwordHasher.VerifyPassword("password123", "HASH:password123").Returns(true);

        var result = await sut.LoginAsync(new LoginRequest("user@example.com", "password123"), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("user@example.com", result!.Email);
    }
}
