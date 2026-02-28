using Backend.Domain;

namespace Backend.Application.Models;

public enum ForecastPeriodMode
{
    RestOfMonth,
    ThisAndNextMonth
}

public sealed class UpsertUserMonthCashflowRequest
{
    public decimal StartBalance { get; set; }
    public decimal SavingsStart { get; set; }
    public decimal WithdrawnFromSavings { get; set; }
    public List<IncomeInput> Incomes { get; set; } = [];
    public List<FixedExpenseInput> FixedExpenses { get; set; } = [];
    public List<VariableExpenseInput> VariableExpenses { get; set; } = [];
    public List<TransactionInput> Transactions { get; set; } = [];
}

public sealed class CashflowForecastRequest
{
    public ForecastPeriodMode PeriodMode { get; set; } = ForecastPeriodMode.RestOfMonth;
    public decimal? DesiredMoneyPerDay { get; set; }
    public List<EmergencyScenarioInput> Scenarios { get; set; } = [];
}

public sealed class IncomeInput
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class FixedExpenseInput
{
    public string Name { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsActive { get; set; } = true;
    public DateOnly? DueDate { get; set; }
    public int? DueDayOfMonth { get; set; }
    public FixedExpenseFrequency Frequency { get; set; } = FixedExpenseFrequency.OneTime;
    public string? Category { get; set; }
}

public sealed class VariableExpenseInput
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class TransactionInput
{
    public DateOnly Date { get; set; }
    public decimal Amount { get; set; }
    public string Label { get; set; } = string.Empty;
}

public sealed class EmergencyScenarioInput
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsTomorrow { get; set; }
}

public sealed class CashflowForecastResult
{
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public decimal CurrentBalance { get; set; }
    public decimal SavingsCurrent { get; set; }
    public decimal WithdrawnFromSavings { get; set; }
    public decimal SpentSoFar { get; set; }
    public decimal BudgetedFixedExpenses { get; set; }
    public decimal BudgetedVariableExpenses { get; set; }
    public decimal BudgetTotal { get; set; }
    public decimal RemainingBudget { get; set; }
    public int DaysLeftTodayInclusive { get; set; }
    public int DaysLeftTomorrowInclusive { get; set; }
    public int DaysLeftYesterdayInclusive { get; set; }
    public decimal? MoneyPerDay { get; set; }
    public decimal? MoneyPerDayTomorrow { get; set; }
    public decimal? MoneyPerDayYesterday { get; set; }
    public decimal? PossibleSavings { get; set; }
    public int? DaysUntilGoal { get; set; }
    public List<EmergencyScenarioResult> Scenarios { get; set; } = [];
}

public sealed class EmergencyScenarioResult
{
    public string Label { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly AssumedDate { get; set; }
    public decimal RemainingBudgetAfterScenario { get; set; }
    public decimal? MoneyPerDayAfterScenario { get; set; }
}
