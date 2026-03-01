namespace Backend.Domain;

public sealed class LedgerTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? Note { get; set; }
    public TransactionKind Kind { get; set; }
    public TransactionStatus Status { get; set; }
    public Guid? RecurringRuleId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
