using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class ExpenseService : IExpenseService
{
    private readonly IExpenseGateway _expenseGateway;

    public ExpenseService(IExpenseGateway expenseGateway)
    {
        _expenseGateway = expenseGateway;
    }

    public async Task<IReadOnlyList<ExpenseDto>> GetExpensesAsync(CancellationToken cancellationToken)
    {
        var expenses = await _expenseGateway.GetAllAsync(cancellationToken);
        return expenses.Select(Map).ToList();
    }

    public async Task<ExpenseDto> AddExpenseAsync(CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Amount = request.Amount,
            Date = request.Date,
            Category = request.Category,
            Note = request.Note,
            CreatedAt = DateTime.UtcNow
        };

        await _expenseGateway.AddAsync(expense, cancellationToken);
        return Map(expense);
    }

    private static ExpenseDto Map(Expense expense) => new(
        expense.Id,
        expense.Amount,
        expense.Date,
        expense.Category,
        expense.Note,
        expense.CreatedAt
    );
}
