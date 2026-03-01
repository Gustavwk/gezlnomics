using Backend.Application.Abstractions;
using Backend.Domain;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Gateways;

public sealed class RecurringRuleGateway : IRecurringRuleGateway
{
    private readonly AppDbContext _dbContext;

    public RecurringRuleGateway(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RecurringRule>> GetAllAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.RecurringRules
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<RecurringRule?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken) =>
        _dbContext.RecurringRules.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<RecurringRule>> GetActiveAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _dbContext.RecurringRules
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(RecurringRule rule, CancellationToken cancellationToken)
    {
        _dbContext.RecurringRules.Add(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(RecurringRule rule, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(RecurringRule rule, CancellationToken cancellationToken)
    {
        _dbContext.RecurringRules.Remove(rule);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await _dbContext.RecurringRules.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        if (items.Count == 0)
        {
            return;
        }

        _dbContext.RecurringRules.RemoveRange(items);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
