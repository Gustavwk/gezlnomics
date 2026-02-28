namespace Backend.Domain;

public enum FixedExpenseFrequency
{
    OneTime,
    Monthly
}

public sealed class UserMonthlyCashflow
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal StartBalance { get; set; }
    public decimal SavingsStart { get; set; }
    public decimal WithdrawnFromSavings { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<UserMonthlyIncome> Incomes { get; set; } = [];
    public List<UserMonthlyFixedExpense> FixedExpenses { get; set; } = [];
    public List<UserMonthlyVariableExpense> VariableExpenses { get; set; } = [];
    public List<UserMonthlyTransaction> Transactions { get; set; } = [];
}

public sealed class UserMonthlyIncome
{
    public Guid Id { get; set; }
    public Guid UserMonthlyCashflowId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class UserMonthlyFixedExpense
{
    public Guid Id { get; set; }
    public Guid UserMonthlyCashflowId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly? DueDate { get; set; }
    public int? DueDayOfMonth { get; set; }
    public FixedExpenseFrequency Frequency { get; set; } = FixedExpenseFrequency.OneTime;
    public string? Category { get; set; }
}

public sealed class UserMonthlyVariableExpense
{
    public Guid Id { get; set; }
    public Guid UserMonthlyCashflowId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class UserMonthlyTransaction
{
    public Guid Id { get; set; }
    public Guid UserMonthlyCashflowId { get; set; }
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}
