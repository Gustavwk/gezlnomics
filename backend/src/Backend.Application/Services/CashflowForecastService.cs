using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class CashflowForecastService : ICashflowForecastService
{
    private readonly IUserMonthlyCashflowGateway _userMonthlyCashflowGateway;

    public CashflowForecastService(IUserMonthlyCashflowGateway userMonthlyCashflowGateway)
    {
        _userMonthlyCashflowGateway = userMonthlyCashflowGateway;
    }

    public async Task SaveUserMonthAsync(Guid userId, int year, int month, UpsertUserMonthCashflowRequest request, CancellationToken cancellationToken)
    {
        var cashflow = await _userMonthlyCashflowGateway.GetByUserMonthAsync(userId, year, month, cancellationToken)
            ?? new UserMonthlyCashflow
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Year = year,
                Month = month,
                CreatedAt = DateTime.UtcNow
            };

        cashflow.StartBalance = request.StartBalance;
        cashflow.SavingsStart = request.SavingsStart;
        cashflow.WithdrawnFromSavings = request.WithdrawnFromSavings;
        cashflow.UpdatedAt = DateTime.UtcNow;

        cashflow.Incomes = request.Incomes.Select(x => new UserMonthlyIncome
        {
            Id = Guid.NewGuid(),
            UserMonthlyCashflowId = cashflow.Id,
            Date = x.Date,
            Amount = x.Amount,
            Label = x.Label
        }).ToList();

        cashflow.FixedExpenses = request.FixedExpenses.Select(x => new UserMonthlyFixedExpense
        {
            Id = Guid.NewGuid(),
            UserMonthlyCashflowId = cashflow.Id,
            Name = x.Name,
            Amount = x.Amount,
            IsActive = x.IsActive,
            DueDate = x.DueDate,
            DueDayOfMonth = x.DueDayOfMonth,
            Frequency = x.Frequency,
            Category = x.Category
        }).ToList();

        cashflow.VariableExpenses = request.VariableExpenses.Select(x => new UserMonthlyVariableExpense
        {
            Id = Guid.NewGuid(),
            UserMonthlyCashflowId = cashflow.Id,
            Date = x.Date,
            Amount = x.Amount,
            Label = x.Label
        }).ToList();

        cashflow.Transactions = request.Transactions.Select(x => new UserMonthlyTransaction
        {
            Id = Guid.NewGuid(),
            UserMonthlyCashflowId = cashflow.Id,
            Date = x.Date,
            Amount = x.Amount,
            Label = x.Label
        }).ToList();

        await _userMonthlyCashflowGateway.SaveAsync(cashflow, cancellationToken);
    }

    public async Task<CashflowForecastResult?> GetForecastAsync(Guid userId, int year, int month, CashflowForecastRequest request, CancellationToken cancellationToken)
    {
        var anchorMonth = await _userMonthlyCashflowGateway.GetByUserMonthAsync(userId, year, month, cancellationToken);
        if (anchorMonth is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var monthStart = new DateOnly(year, month, 1);
        var anchorDate = monthStart.Year == today.Year && monthStart.Month == today.Month ? today : monthStart;
        var periodEnd = ResolvePeriodEnd(monthStart, request.PeriodMode);

        var monthsInPeriod = await _userMonthlyCashflowGateway.GetByUserInPeriodAsync(userId, anchorDate, periodEnd, cancellationToken);

        var allIncomes = monthsInPeriod.SelectMany(x => x.Incomes).ToList();
        var allFixedExpenses = monthsInPeriod.SelectMany(x => x.FixedExpenses).ToList();
        var allVariableExpenses = monthsInPeriod.SelectMany(x => x.VariableExpenses).ToList();

        var receivedIncomes = allIncomes.Where(x => x.Date <= anchorDate).Sum(x => x.Amount);
        var futureIncomes = allIncomes.Where(x => x.Date > anchorDate && x.Date <= periodEnd).Sum(x => x.Amount);

        var currentBalance = anchorMonth.StartBalance + anchorMonth.WithdrawnFromSavings + receivedIncomes;
        var savingsCurrent = anchorMonth.SavingsStart - anchorMonth.WithdrawnFromSavings;

        var spentSoFar = anchorMonth.Transactions
            .Where(x => x.Date >= monthStart && x.Date <= anchorDate)
            .Sum(x => x.Amount);

        var budgetedFixedExpenses = allFixedExpenses
            .Where(x => x.IsActive)
            .Sum(x => x.Amount * CountOccurrencesInPeriod(x, anchorDate, periodEnd));

        var budgetedVariableExpenses = allVariableExpenses
            .Where(x => x.Date >= anchorDate && x.Date <= periodEnd)
            .Sum(x => x.Amount);

        var budgetTotal = budgetedFixedExpenses + budgetedVariableExpenses;
        var remainingBudget = currentBalance - spentSoFar - budgetTotal + futureIncomes;

        var daysLeftTodayInclusive = CountDaysInclusive(anchorDate, periodEnd);
        var daysLeftTomorrowInclusive = CountDaysInclusive(anchorDate.AddDays(1), periodEnd);
        var daysLeftYesterdayInclusive = CountDaysInclusive(anchorDate.AddDays(-1), periodEnd);

        var moneyPerDay = DivideOrNull(remainingBudget, daysLeftTodayInclusive);
        var moneyPerDayTomorrow = DivideOrNull(remainingBudget, daysLeftTomorrowInclusive);
        var moneyPerDayYesterday = DivideOrNull(remainingBudget, daysLeftYesterdayInclusive);

        var possibleSavings = request.DesiredMoneyPerDay.HasValue
            ? remainingBudget - request.DesiredMoneyPerDay.Value * daysLeftTodayInclusive
            : null;

        int? daysUntilGoal = null;
        if (request.DesiredMoneyPerDay is > 0)
        {
            daysUntilGoal = (int)Math.Floor(remainingBudget / request.DesiredMoneyPerDay.Value);
        }

        var scenarios = BuildScenarioResults(request.Scenarios, anchorDate, periodEnd, remainingBudget);

        return new CashflowForecastResult
        {
            UserId = userId,
            Year = year,
            Month = month,
            PeriodStart = anchorDate,
            PeriodEnd = periodEnd,
            CurrentBalance = currentBalance,
            SavingsCurrent = savingsCurrent,
            WithdrawnFromSavings = anchorMonth.WithdrawnFromSavings,
            SpentSoFar = spentSoFar,
            BudgetedFixedExpenses = budgetedFixedExpenses,
            BudgetedVariableExpenses = budgetedVariableExpenses,
            BudgetTotal = budgetTotal,
            RemainingBudget = remainingBudget,
            DaysLeftTodayInclusive = daysLeftTodayInclusive,
            DaysLeftTomorrowInclusive = daysLeftTomorrowInclusive,
            DaysLeftYesterdayInclusive = daysLeftYesterdayInclusive,
            MoneyPerDay = moneyPerDay,
            MoneyPerDayTomorrow = moneyPerDayTomorrow,
            MoneyPerDayYesterday = moneyPerDayYesterday,
            PossibleSavings = possibleSavings,
            DaysUntilGoal = daysUntilGoal,
            Scenarios = scenarios
        };
    }

    private static DateOnly ResolvePeriodEnd(DateOnly monthStart, ForecastPeriodMode periodMode)
    {
        var target = periodMode == ForecastPeriodMode.ThisAndNextMonth ? monthStart.AddMonths(1) : monthStart;
        return EndOfMonth(target.Year, target.Month);
    }

    private static int CountOccurrencesInPeriod(UserMonthlyFixedExpense item, DateOnly periodStart, DateOnly periodEnd)
    {
        if (periodEnd < periodStart)
        {
            return 0;
        }

        if (item.Frequency == FixedExpenseFrequency.OneTime)
        {
            if (item.DueDate is null)
            {
                return 0;
            }

            return item.DueDate.Value >= periodStart && item.DueDate.Value <= periodEnd ? 1 : 0;
        }

        if (item.DueDayOfMonth is null)
        {
            return 0;
        }

        var count = 0;
        var cursor = new DateOnly(periodStart.Year, periodStart.Month, 1);
        var endMonth = new DateOnly(periodEnd.Year, periodEnd.Month, 1);

        while (cursor <= endMonth)
        {
            var dueDay = Math.Min(item.DueDayOfMonth.Value, DateTime.DaysInMonth(cursor.Year, cursor.Month));
            var dueDate = new DateOnly(cursor.Year, cursor.Month, dueDay);
            if (dueDate >= periodStart && dueDate <= periodEnd)
            {
                count++;
            }

            cursor = cursor.AddMonths(1);
        }

        return count;
    }

    private static List<EmergencyScenarioResult> BuildScenarioResults(List<EmergencyScenarioInput> scenarios, DateOnly anchorDate, DateOnly periodEnd, decimal remainingBudget)
    {
        var effectiveScenarios = scenarios.Count == 0
            ?
            [
                new EmergencyScenarioInput { Label = "Emergency 250 tomorrow", Amount = 250, IsTomorrow = true },
                new EmergencyScenarioInput { Label = "Emergency 500 tomorrow", Amount = 500, IsTomorrow = true },
                new EmergencyScenarioInput { Label = "Emergency 1000 tomorrow", Amount = 1000, IsTomorrow = true },
                new EmergencyScenarioInput { Label = "Emergency 2000 tomorrow", Amount = 2000, IsTomorrow = true },
                new EmergencyScenarioInput { Label = "Emergency 3000 tomorrow", Amount = 3000, IsTomorrow = true },
                new EmergencyScenarioInput { Label = "Emergency 4000 tomorrow", Amount = 4000, IsTomorrow = true }
            ]
            : scenarios;

        return effectiveScenarios.Select(s =>
        {
            var assumedDate = s.IsTomorrow ? anchorDate.AddDays(1) : anchorDate;
            var days = s.IsTomorrow ? CountDaysInclusive(anchorDate.AddDays(1), periodEnd) : CountDaysInclusive(anchorDate, periodEnd);
            var remainingBudgetAfterScenario = remainingBudget - s.Amount;

            return new EmergencyScenarioResult
            {
                Label = s.Label,
                Amount = s.Amount,
                AssumedDate = assumedDate,
                RemainingBudgetAfterScenario = remainingBudgetAfterScenario,
                MoneyPerDayAfterScenario = DivideOrNull(remainingBudgetAfterScenario, days)
            };
        }).ToList();
    }

    private static int CountDaysInclusive(DateOnly start, DateOnly end)
    {
        return end.DayNumber - start.DayNumber + 1;
    }

    private static decimal? DivideOrNull(decimal numerator, int denominator)
    {
        if (denominator <= 0)
        {
            return null;
        }

        return Math.Round(numerator / denominator, 2, MidpointRounding.AwayFromZero);
    }

    private static DateOnly EndOfMonth(int year, int month)
    {
        return new DateOnly(year, month, DateTime.DaysInMonth(year, month));
    }
}
