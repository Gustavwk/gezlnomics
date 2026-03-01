namespace Backend.Application.Models;

public sealed record SignupRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthUserDto(Guid Id, string Email);
