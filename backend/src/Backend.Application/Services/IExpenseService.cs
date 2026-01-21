using Backend.Application.Models;

namespace Backend.Application.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetExpensesAsync(CancellationToken cancellationToken);
    Task<ExpenseDto> AddExpenseAsync(CreateExpenseRequest request, CancellationToken cancellationToken);
}
