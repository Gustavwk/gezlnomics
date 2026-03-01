using Backend.Domain;

namespace Backend.Application.Models;

public sealed record AccountExportDto(
    UserSettingsDto? Settings,
    IReadOnlyList<IncomePeriodDto> IncomePeriods,
    IReadOnlyList<TransactionDto> Transactions,
    IReadOnlyList<RecurringRuleDto> RecurringRules
);
