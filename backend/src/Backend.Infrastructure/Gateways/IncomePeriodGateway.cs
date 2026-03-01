using Backend.Application.Abstractions;
using Backend.Domain;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Gateways;

public sealed class IncomePeriodGateway : IIncomePeriodGateway
{
    private readonly AppDbContext _dbContext;

    public IncomePeriodGateway(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<IncomePeriod>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var query = _dbContext.IncomePeriods.AsNoTracking().Where(x => x.UserId == userId);

        if (from.HasValue)
        {
            query = query.Where(x => x.PeriodEndDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.PeriodStartDate <= to.Value);
        }

        return await query.OrderByDescending(x => x.PeriodStartDate).ToListAsync(cancellationToken);
    }

    public Task<IncomePeriod?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken) =>
        _dbContext.IncomePeriods.FirstOrDefaultAsync(x => x.UserId == userId && x.Id == id, cancellationToken);

    public Task<IncomePeriod?> GetForPeriodAsync(Guid userId, DateOnly periodStart, DateOnly periodEnd, CancellationToken cancellationToken) =>
        _dbContext.IncomePeriods.FirstOrDefaultAsync(x => x.UserId == userId && x.PeriodStartDate == periodStart && x.PeriodEndDate == periodEnd, cancellationToken);

    public async Task AddAsync(IncomePeriod period, CancellationToken cancellationToken)
    {
        _dbContext.IncomePeriods.Add(period);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(IncomePeriod period, CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(IncomePeriod period, CancellationToken cancellationToken)
    {
        _dbContext.IncomePeriods.Remove(period);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var items = await _dbContext.IncomePeriods.Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        if (items.Count == 0)
        {
            return;
        }

        _dbContext.IncomePeriods.RemoveRange(items);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
