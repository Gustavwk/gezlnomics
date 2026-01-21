namespace Backend.Application.Models;

public sealed record ExpenseDto(
    Guid Id,
    decimal Amount,
    DateOnly Date,
    string Category,
    string? Note,
    DateTime CreatedAt
);
