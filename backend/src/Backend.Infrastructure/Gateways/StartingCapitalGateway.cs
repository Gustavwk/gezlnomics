using Backend.Application.Abstractions;
using Backend.Domain;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Gateways;

public sealed class StartingCapitalGateway : IStartingCapitalGateway
{
    private readonly AppDbContext _dbContext;

    public StartingCapitalGateway(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StartingCapital?> GetLatestAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.StartingCapitals
            .AsNoTracking()
            .OrderByDescending(sc => sc.Date)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AddAsync(StartingCapital startingCapital, CancellationToken cancellationToken)
    {
        _dbContext.StartingCapitals.Add(startingCapital);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
