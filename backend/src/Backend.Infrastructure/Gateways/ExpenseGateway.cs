using Backend.Application.Abstractions;
using Backend.Domain;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Gateways;

public sealed class ExpenseGateway : IExpenseGateway
{
    private readonly AppDbContext _dbContext;

    public ExpenseGateway(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Expenses
            .AsNoTracking()
            .OrderByDescending(e => e.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Expense expense, CancellationToken cancellationToken)
    {
        _dbContext.Expenses.Add(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
