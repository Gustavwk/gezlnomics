using Backend.Application.Abstractions;
using Backend.Domain;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Gateways;

public sealed class TransactionGateway : ITransactionGateway
{
    private readonly AppDbContext _dbContext;

    public TransactionGateway(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LedgerTransaction>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, TransactionKind? kind, string? category, string? query, CancellationToken cancellationToken)
    {
        var queryable = _dbContext.Transactions.AsNoTracking().Where(x => x.UserId == userId);

        if (from.HasValue)
        {
            queryable = queryable.Where(x => x.Date >= from.Value);
        }

        if (to.HasValue)
        {
            queryable = queryable.Where(x => x.Date <= to.Value);
        }

        if (kind.HasValue)
        {
            queryable = queryable.Where(x => x.Kind == kind.Value);
        }

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalized = category.Trim().ToLower();
            queryable = queryable.Where(x => x.Category.ToLower() == normalized);
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            var q = query.Trim().ToLower();
            queryable = queryable.Where(x => x.Category.ToLower().Contains(q) || (x.Note != null && x.Note.ToLower().Contains(q)));
        }

        return await queryable.OrderByDescending(x => x.Date).ThenByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<LedgerTransaction>> GetRangeAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        return await _dbContext.Transactions
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Date >= from && x.Date <= to)
            .OrderBy(x => x.Date)
            .ThenBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<LedgerTransaction?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken) =>
        _dbContext.Transactions.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);

    public async Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken)
    {
        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(LedgerTransaction transaction, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(LedgerTransaction transaction, CancellationToken cancellationToken)
    {
        _dbContext.Transactions.Remove(transaction);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
