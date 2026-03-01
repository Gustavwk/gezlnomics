using Backend.Domain;

namespace Backend.Application.Models;

public sealed record RecurringRuleDto(
    Guid Id,
    string Title,
    decimal Amount,
    string Category,
    string? Note,
    TransactionKind RuleKind,
    RecurringFrequency Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public sealed record UpsertRecurringRuleRequest(
    string Title,
    decimal Amount,
    string Category,
    string? Note,
    TransactionKind RuleKind,
    RecurringFrequency Frequency,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive
);
