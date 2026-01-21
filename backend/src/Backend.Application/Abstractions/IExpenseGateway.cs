using Backend.Domain;

namespace Backend.Application.Abstractions;

public interface IExpenseGateway
{
    Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Expense expense, CancellationToken cancellationToken);
}
