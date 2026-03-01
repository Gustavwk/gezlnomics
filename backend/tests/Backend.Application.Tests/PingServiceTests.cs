using Backend.Application.Services;

namespace Backend.Application.Tests;

public sealed class PingServiceTests
{
    [Fact]
    public void GetPing_ReturnsPong()
    {
        var sut = new PingService();

        var result = sut.GetPing();

        Assert.Equal("pong", result);
    }
}
