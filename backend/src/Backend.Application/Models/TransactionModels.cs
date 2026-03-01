using Backend.Domain;

namespace Backend.Application.Models;

public sealed record TransactionDto(
    Guid Id,
    DateOnly Date,
    decimal Amount,
    string Category,
    string? Note,
    TransactionKind Kind,
    TransactionStatus Status,
    Guid? RecurringRuleId,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record UpsertTransactionRequest(
    DateOnly Date,
    decimal Amount,
    string Category,
    string? Note,
    TransactionKind Kind,
    TransactionStatus Status
);
