using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class AccountService : IAccountService
{
    private readonly IUserGateway _userGateway;
    private readonly IUserSettingsGateway _settingsGateway;
    private readonly IIncomePeriodGateway _incomePeriodGateway;
    private readonly ITransactionGateway _transactionGateway;
    private readonly IRecurringRuleGateway _recurringRuleGateway;

    public AccountService(
        IUserGateway userGateway,
        IUserSettingsGateway settingsGateway,
        IIncomePeriodGateway incomePeriodGateway,
        ITransactionGateway transactionGateway,
        IRecurringRuleGateway recurringRuleGateway)
    {
        _userGateway = userGateway;
        _settingsGateway = settingsGateway;
        _incomePeriodGateway = incomePeriodGateway;
        _transactionGateway = transactionGateway;
        _recurringRuleGateway = recurringRuleGateway;
    }

    public async Task<AccountExportDto?> ExportAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userGateway.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var settings = await _settingsGateway.GetByUserIdAsync(userId, cancellationToken);
        var periods = await _incomePeriodGateway.GetAllAsync(userId, null, null, cancellationToken);
        var transactions = await _transactionGateway.GetAllAsync(userId, null, null, null, null, null, cancellationToken);
        var recurringRules = await _recurringRuleGateway.GetAllAsync(userId, cancellationToken);

        return new AccountExportDto(
            settings is null ? null : new UserSettingsDto(settings.PaydayDayOfMonth, settings.CurrencyCode, settings.Timezone),
            periods.Select(p => new IncomePeriodDto(p.Id, p.PeriodStartDate, p.PeriodEndDate, p.StartingBalance, p.CreatedAt)).ToList(),
            transactions.Select(t => new TransactionDto(
                t.Id,
                t.Date,
                t.Amount,
                t.Category,
                t.Note,
                t.Kind,
                t.Status,
                t.RecurringRuleId,
                t.CreatedAt,
                t.UpdatedAt)).ToList(),
            recurringRules.Select(r => new RecurringRuleDto(
                r.Id,
                r.Title,
                r.Amount,
                r.Category,
                r.Note,
                r.RuleKind,
                r.Frequency,
                r.StartDate,
                r.EndDate,
                r.IsActive,
                r.CreatedAt,
                r.UpdatedAt)).ToList()
        );
    }

    public async Task<bool> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userGateway.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return false;
        }

        await _transactionGateway.DeleteAllByUserIdAsync(userId, cancellationToken);
        await _recurringRuleGateway.DeleteAllByUserIdAsync(userId, cancellationToken);
        await _incomePeriodGateway.DeleteAllByUserIdAsync(userId, cancellationToken);
        await _settingsGateway.DeleteByUserIdAsync(userId, cancellationToken);
        await _userGateway.DeleteAsync(user, cancellationToken);

        return true;
    }
}
