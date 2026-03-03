namespace Backend.Application.Models;

public sealed record SignupRequest(string Username, string Password);
public sealed record LoginRequest(string Username, string Password);
public sealed record AuthUserDto(Guid Id, string Username);
