using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class LedgerService : ILedgerService
{
    private readonly IUserSettingsGateway _settingsGateway;
    private readonly IIncomePeriodGateway _incomePeriodGateway;
    private readonly ITransactionGateway _transactionGateway;
    private readonly IRecurringRuleGateway _recurringRuleGateway;

    public LedgerService(
        IUserSettingsGateway settingsGateway,
        IIncomePeriodGateway incomePeriodGateway,
        ITransactionGateway transactionGateway,
        IRecurringRuleGateway recurringRuleGateway)
    {
        _settingsGateway = settingsGateway;
        _incomePeriodGateway = incomePeriodGateway;
        _transactionGateway = transactionGateway;
        _recurringRuleGateway = recurringRuleGateway;
    }

    public async Task<LedgerSummaryDto> GetSummaryAsync(Guid userId, DateOnly asOf, CancellationToken cancellationToken)
    {
        var settings = await _settingsGateway.GetByUserIdAsync(userId, cancellationToken) ??
            new UserSettings { UserId = userId, PaydayDayOfMonth = 1, CurrencyCode = "DKK", Timezone = "Europe/Copenhagen" };

        var (periodStart, periodEnd, nextPayday) = ResolvePeriod(asOf, settings.PaydayDayOfMonth);
        var incomePeriod = await _incomePeriodGateway.GetForPeriodAsync(userId, periodStart, periodEnd, cancellationToken);
        var startingBalance = incomePeriod?.StartingBalance ?? 0m;

        var transactions = await _transactionGateway.GetRangeAsync(userId, periodStart, periodEnd, cancellationToken);
        var recurringRules = await _recurringRuleGateway.GetActiveAsync(userId, cancellationToken);

        var currentDelta = transactions
            .Where(t => t.Status == TransactionStatus.Active && t.Date <= asOf && t.Kind != TransactionKind.ExpensePlanned)
            .Sum(SignedAmount);

        var futureDelta = transactions
            .Where(t => t.Status == TransactionStatus.Active && t.Date > asOf)
            .Sum(SignedAmount);

        var recurringCurrentDelta = CalculateRecurringDelta(recurringRules, periodStart, asOf);
        var recurringFutureDelta = CalculateRecurringDelta(recurringRules, asOf.AddDays(1), periodEnd);

        var currentBalance = startingBalance + currentDelta + recurringCurrentDelta;
        var forecastBalance = currentBalance + futureDelta + recurringFutureDelta;
        var daysUntilNextPayday = Math.Max(1, nextPayday.DayNumber - asOf.DayNumber);
        var moneyPerDay = Math.Round(Math.Max(0m, forecastBalance) / daysUntilNextPayday, 2, MidpointRounding.AwayFromZero);

        var cumulativeSpending = transactions
            .Where(t => t.Status == TransactionStatus.Active && t.Date >= periodStart && t.Date <= asOf)
            .Sum(t => t.Kind switch
            {
                TransactionKind.ExpenseActual or TransactionKind.ExpensePlanned or TransactionKind.SavingsTransferOut => t.Amount,
                _ => 0m
            });

        var recurringSpending = Math.Abs(CalculateRecurringDelta(
            recurringRules.Where(r => r.RuleKind is TransactionKind.ExpenseActual or TransactionKind.ExpensePlanned or TransactionKind.SavingsTransferOut).ToList(),
            periodStart,
            asOf));

        return new LedgerSummaryDto(
            periodStart,
            periodEnd,
            startingBalance,
            currentBalance,
            forecastBalance,
            daysUntilNextPayday,
            moneyPerDay,
            cumulativeSpending + recurringSpending,
            settings.CurrencyCode
        );
    }

    public async Task<IReadOnlyList<LedgerTimelinePointDto>> GetTimelineAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        if (to < from)
        {
            throw new InvalidOperationException("to skal være efter from.");
        }

        var points = new List<LedgerTimelinePointDto>();
        for (var date = from; date <= to; date = date.AddDays(1))
        {
            var summary = await GetSummaryAsync(userId, date, cancellationToken);
            points.Add(new LedgerTimelinePointDto(date, summary.CurrentBalance, summary.ForecastBalance, summary.CumulativeSpending));
        }

        return points;
    }

    private static decimal CalculateRecurringDelta(IReadOnlyList<RecurringRule> rules, DateOnly from, DateOnly to)
    {
        decimal delta = 0m;
        foreach (var rule in rules.Where(r => r.IsActive))
        {
            foreach (var _ in EnumerateOccurrences(rule, from, to))
            {
                delta += SignedAmount(rule.RuleKind, rule.Amount);
            }
        }

        return delta;
    }

    private static IEnumerable<DateOnly> EnumerateOccurrences(RecurringRule rule, DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            yield break;
        }

        var cursor = rule.StartDate;
        while (cursor < from)
        {
            cursor = NextDate(cursor, rule.Frequency);
            if (rule.EndDate.HasValue && cursor > rule.EndDate.Value)
            {
                yield break;
            }
        }

        while (cursor <= to)
        {
            if (!rule.EndDate.HasValue || cursor <= rule.EndDate.Value)
            {
                yield return cursor;
            }

            cursor = NextDate(cursor, rule.Frequency);
            if (rule.EndDate.HasValue && cursor > rule.EndDate.Value)
            {
                yield break;
            }
        }
    }

    private static DateOnly NextDate(DateOnly date, RecurringFrequency frequency) => frequency switch
    {
        RecurringFrequency.Weekly => date.AddDays(7),
        RecurringFrequency.Monthly => date.AddMonths(1),
        RecurringFrequency.Yearly => date.AddYears(1),
        _ => date.AddMonths(1)
    };

    private static (DateOnly PeriodStart, DateOnly PeriodEnd, DateOnly NextPayday) ResolvePeriod(DateOnly asOf, int paydayDay)
    {
        var safeDayCurrentMonth = Math.Min(paydayDay, DateTime.DaysInMonth(asOf.Year, asOf.Month));
        var thisMonthPayday = new DateOnly(asOf.Year, asOf.Month, safeDayCurrentMonth);

        DateOnly periodStart;
        DateOnly nextPayday;

        if (asOf >= thisMonthPayday)
        {
            periodStart = thisMonthPayday;
            var nextMonth = asOf.AddMonths(1);
            var safeDayNextMonth = Math.Min(paydayDay, DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month));
            nextPayday = new DateOnly(nextMonth.Year, nextMonth.Month, safeDayNextMonth);
        }
        else
        {
            var prevMonth = asOf.AddMonths(-1);
            var safeDayPrevMonth = Math.Min(paydayDay, DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month));
            periodStart = new DateOnly(prevMonth.Year, prevMonth.Month, safeDayPrevMonth);
            nextPayday = thisMonthPayday;
        }

        var periodEnd = nextPayday.AddDays(-1);
        return (periodStart, periodEnd, nextPayday);
    }

    private static decimal SignedAmount(LedgerTransaction transaction) => SignedAmount(transaction.Kind, transaction.Amount);

    private static decimal SignedAmount(TransactionKind kind, decimal amount) => kind switch
    {
        TransactionKind.ExpenseActual => -Math.Abs(amount),
        TransactionKind.ExpensePlanned => -Math.Abs(amount),
        TransactionKind.SavingsTransferOut => -Math.Abs(amount),
        TransactionKind.SavingsTransferIn => Math.Abs(amount),
        _ => 0m
    };
}
