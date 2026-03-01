using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public interface ITransactionService
{
    Task<IReadOnlyList<TransactionDto>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, TransactionKind? kind, string? category, string? query, CancellationToken cancellationToken);
    Task<TransactionDto> CreateAsync(Guid userId, UpsertTransactionRequest request, CancellationToken cancellationToken);
    Task<TransactionDto?> UpdateAsync(Guid userId, Guid id, UpsertTransactionRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
