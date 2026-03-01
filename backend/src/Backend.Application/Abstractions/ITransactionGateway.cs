using Backend.Domain;

namespace Backend.Application.Abstractions;

public interface ITransactionGateway
{
    Task<IReadOnlyList<LedgerTransaction>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, TransactionKind? kind, string? category, string? query, CancellationToken cancellationToken);
    Task<IReadOnlyList<LedgerTransaction>> GetRangeAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken);
    Task<LedgerTransaction?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task AddAsync(LedgerTransaction transaction, CancellationToken cancellationToken);
    Task UpdateAsync(LedgerTransaction transaction, CancellationToken cancellationToken);
    Task DeleteAsync(LedgerTransaction transaction, CancellationToken cancellationToken);
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
