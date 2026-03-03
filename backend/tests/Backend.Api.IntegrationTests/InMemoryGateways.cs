using Backend.Application.Abstractions;
using Backend.Domain;

namespace Backend.Api.IntegrationTests;

internal sealed class InMemoryDataStore
{
    public List<User> Users { get; } = new();
    public List<UserSettings> Settings { get; } = new();
    public List<IncomePeriod> IncomePeriods { get; } = new();
    public List<LedgerTransaction> Transactions { get; } = new();
    public List<RecurringRule> RecurringRules { get; } = new();
}

internal sealed class InMemoryUserGateway : IUserGateway
{
    private readonly InMemoryDataStore _store;

    public InMemoryUserGateway(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken)
        => Task.FromResult(_store.Users.FirstOrDefault(x => x.Username == username));

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => Task.FromResult(_store.Users.FirstOrDefault(x => x.Id == id));

    public Task AddAsync(User user, CancellationToken cancellationToken)
    {
        _store.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user, CancellationToken cancellationToken)
    {
        _store.Users.Remove(user);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryUserSettingsGateway : IUserSettingsGateway
{
    private readonly InMemoryDataStore _store;

    public InMemoryUserSettingsGateway(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<UserSettings?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => Task.FromResult(_store.Settings.FirstOrDefault(x => x.UserId == userId));

    public Task UpsertAsync(UserSettings settings, CancellationToken cancellationToken)
    {
        var existing = _store.Settings.FirstOrDefault(x => x.UserId == settings.UserId);
        if (existing is null)
        {
            _store.Settings.Add(settings);
        }
        else
        {
            existing.PaydayDayOfMonth = settings.PaydayDayOfMonth;
            existing.CurrencyCode = settings.CurrencyCode;
            existing.Timezone = settings.Timezone;
            existing.UpdatedAt = settings.UpdatedAt;
        }

        return Task.CompletedTask;
    }

    public Task DeleteByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        _store.Settings.RemoveAll(x => x.UserId == userId);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryIncomePeriodGateway : IIncomePeriodGateway
{
    private readonly InMemoryDataStore _store;

    public InMemoryIncomePeriodGateway(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<IncomePeriod>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var query = _store.IncomePeriods.Where(x => x.UserId == userId);
        if (from.HasValue)
        {
            query = query.Where(x => x.PeriodEndDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.PeriodStartDate <= to.Value);
        }

        return Task.FromResult<IReadOnlyList<IncomePeriod>>(query.OrderByDescending(x => x.PeriodStartDate).ToList());
    }

    public Task<IncomePeriod?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        => Task.FromResult(_store.IncomePeriods.FirstOrDefault(x => x.UserId == userId && x.Id == id));

    public Task<IncomePeriod?> GetForPeriodAsync(Guid userId, DateOnly periodStart, DateOnly periodEnd, CancellationToken cancellationToken)
        => Task.FromResult(_store.IncomePeriods.FirstOrDefault(x => x.UserId == userId && x.PeriodStartDate == periodStart && x.PeriodEndDate == periodEnd));

    public Task AddAsync(IncomePeriod period, CancellationToken cancellationToken)
    {
        _store.IncomePeriods.Add(period);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(IncomePeriod period, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task DeleteAsync(IncomePeriod period, CancellationToken cancellationToken)
    {
        _store.IncomePeriods.Remove(period);
        return Task.CompletedTask;
    }

    public Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        _store.IncomePeriods.RemoveAll(x => x.UserId == userId);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryTransactionGateway : ITransactionGateway
{
    private readonly InMemoryDataStore _store;

    public InMemoryTransactionGateway(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<LedgerTransaction>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, TransactionKind? kind, string? category, string? query, CancellationToken cancellationToken)
    {
        IEnumerable<LedgerTransaction> result = _store.Transactions.Where(x => x.UserId == userId);

        if (from.HasValue)
        {
            result = result.Where(x => x.Date >= from.Value);
        }

        if (to.HasValue)
        {
            result = result.Where(x => x.Date <= to.Value);
        }

        if (kind.HasValue)
        {
            result = result.Where(x => x.Kind == kind.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalized = category.Trim().ToLowerInvariant();
            result = result.Where(x => x.Category.ToLowerInvariant() == normalized);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalizedQuery = query.Trim().ToLowerInvariant();
            result = result.Where(x => x.Category.ToLowerInvariant().Contains(normalizedQuery) || (x.Note is not null && x.Note.ToLowerInvariant().Contains(normalizedQuery)));
        }

        return Task.FromResult<IReadOnlyList<LedgerTransaction>>(result.OrderByDescending(x => x.Date).ThenByDescending(x => x.CreatedAt).ToList());
    }

    public Task<IReadOnlyList<LedgerTransaction>> GetRangeAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<LedgerTransaction>>(_store.Transactions.Where(x => x.UserId == userId && x.Date >= from && x.Date <= to).OrderBy(x => x.Date).ThenBy(x => x.CreatedAt).ToList());

    public Task<LedgerTransaction?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        => Task.FromResult(_store.Transactions.FirstOrDefault(x => x.UserId == userId && x.Id == id));

    public Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken)
    {
        _store.Transactions.Add(transaction);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(LedgerTransaction transaction, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task DeleteAsync(LedgerTransaction transaction, CancellationToken cancellationToken)
    {
        _store.Transactions.Remove(transaction);
        return Task.CompletedTask;
    }

    public Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        _store.Transactions.RemoveAll(x => x.UserId == userId);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryRecurringRuleGateway : IRecurringRuleGateway
{
    private readonly InMemoryDataStore _store;

    public InMemoryRecurringRuleGateway(InMemoryDataStore store)
    {
        _store = store;
    }

    public Task<IReadOnlyList<RecurringRule>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<RecurringRule>>(_store.RecurringRules.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToList());

    public Task<RecurringRule?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken)
        => Task.FromResult(_store.RecurringRules.FirstOrDefault(x => x.UserId == userId && x.Id == id));

    public Task<IReadOnlyList<RecurringRule>> GetActiveAsync(Guid userId, CancellationToken cancellationToken)
        => Task.FromResult<IReadOnlyList<RecurringRule>>(_store.RecurringRules.Where(x => x.UserId == userId && x.IsActive).ToList());

    public Task AddAsync(RecurringRule rule, CancellationToken cancellationToken)
    {
        _store.RecurringRules.Add(rule);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(RecurringRule rule, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task DeleteAsync(RecurringRule rule, CancellationToken cancellationToken)
    {
        _store.RecurringRules.Remove(rule);
        return Task.CompletedTask;
    }

    public Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        _store.RecurringRules.RemoveAll(x => x.UserId == userId);
        return Task.CompletedTask;
    }
}
