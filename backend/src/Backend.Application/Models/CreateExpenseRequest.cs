namespace Backend.Application.Models;

public sealed record CreateExpenseRequest(
    decimal Amount,
    DateOnly Date,
    string Category,
    string? Note
);
