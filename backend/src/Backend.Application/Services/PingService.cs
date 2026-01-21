namespace Backend.Application.Services;

public sealed class PingService : IPingService
{
    public string GetPing() => "pong";
}
