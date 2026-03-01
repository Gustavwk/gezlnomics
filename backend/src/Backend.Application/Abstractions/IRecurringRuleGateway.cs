using Backend.Domain;

namespace Backend.Application.Abstractions;

public interface IRecurringRuleGateway
{
    Task<IReadOnlyList<RecurringRule>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<RecurringRule?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<RecurringRule>> GetActiveAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(RecurringRule rule, CancellationToken cancellationToken);
    Task UpdateAsync(RecurringRule rule, CancellationToken cancellationToken);
    Task DeleteAsync(RecurringRule rule, CancellationToken cancellationToken);
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
