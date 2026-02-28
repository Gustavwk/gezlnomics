using Backend.Application.Abstractions;
using Backend.Domain;
using Backend.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Infrastructure.Gateways;

public sealed class UserMonthlyCashflowGateway : IUserMonthlyCashflowGateway
{
    private readonly AppDbContext _dbContext;

    public UserMonthlyCashflowGateway(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserMonthlyCashflow?> GetByUserMonthAsync(Guid userId, int year, int month, CancellationToken cancellationToken)
    {
        return await _dbContext.UserMonthlyCashflows
            .Include(x => x.Incomes)
            .Include(x => x.FixedExpenses)
            .Include(x => x.VariableExpenses)
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.Year == year && x.Month == month, cancellationToken);
    }

    public async Task<IReadOnlyList<UserMonthlyCashflow>> GetByUserInPeriodAsync(Guid userId, DateOnly start, DateOnly end, CancellationToken cancellationToken)
    {
        return await _dbContext.UserMonthlyCashflows
            .Include(x => x.Incomes)
            .Include(x => x.FixedExpenses)
            .Include(x => x.VariableExpenses)
            .Include(x => x.Transactions)
            .Where(x => x.UserId == userId && IsMonthInRange(x.Year, x.Month, start, end))
            .ToListAsync(cancellationToken);
    }

    public async Task SaveAsync(UserMonthlyCashflow cashflow, CancellationToken cancellationToken)
    {
        var existing = await _dbContext.UserMonthlyCashflows
            .Include(x => x.Incomes)
            .Include(x => x.FixedExpenses)
            .Include(x => x.VariableExpenses)
            .Include(x => x.Transactions)
            .FirstOrDefaultAsync(x => x.Id == cashflow.Id, cancellationToken);

        if (existing is null)
        {
            _dbContext.UserMonthlyCashflows.Add(cashflow);
        }
        else
        {
            existing.StartBalance = cashflow.StartBalance;
            existing.SavingsStart = cashflow.SavingsStart;
            existing.WithdrawnFromSavings = cashflow.WithdrawnFromSavings;
            existing.UpdatedAt = cashflow.UpdatedAt;

            _dbContext.UserMonthlyIncomes.RemoveRange(existing.Incomes);
            _dbContext.UserMonthlyFixedExpenses.RemoveRange(existing.FixedExpenses);
            _dbContext.UserMonthlyVariableExpenses.RemoveRange(existing.VariableExpenses);
            _dbContext.UserMonthlyTransactions.RemoveRange(existing.Transactions);

            existing.Incomes = cashflow.Incomes;
            existing.FixedExpenses = cashflow.FixedExpenses;
            existing.VariableExpenses = cashflow.VariableExpenses;
            existing.Transactions = cashflow.Transactions;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static bool IsMonthInRange(int year, int month, DateOnly start, DateOnly end)
    {
        var monthDate = new DateOnly(year, month, 1);
        var endOfMonth = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        return endOfMonth >= start && monthDate <= end;
    }
}
