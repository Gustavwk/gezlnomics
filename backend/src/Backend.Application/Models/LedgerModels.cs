namespace Backend.Application.Models;

public sealed record LedgerSummaryDto(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal StartingBalance,
    decimal CurrentBalance,
    decimal ForecastBalance,
    int DaysUntilNextPayday,
    decimal MoneyPerDay,
    decimal CumulativeSpending,
    string CurrencyCode
);

public sealed record LedgerTimelinePointDto(DateOnly Date, decimal RunningActual, decimal RunningForecast, decimal CumulativeSpending);
