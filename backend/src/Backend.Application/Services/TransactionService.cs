using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class TransactionService : ITransactionService
{
    private readonly ITransactionGateway _gateway;

    public TransactionService(ITransactionGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<IReadOnlyList<TransactionDto>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, TransactionKind? kind, string? category, string? query, CancellationToken cancellationToken)
    {
        var transactions = await _gateway.GetAllAsync(userId, from, to, kind, category, query, cancellationToken);
        return transactions.Select(Map).ToList();
    }

    public async Task<TransactionDto> CreateAsync(Guid userId, UpsertTransactionRequest request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var transaction = new LedgerTransaction
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = request.Date,
            Amount = Math.Abs(request.Amount),
            Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim(),
            Note = request.Note?.Trim(),
            Kind = request.Kind,
            Status = request.Status,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _gateway.AddAsync(transaction, cancellationToken);
        return Map(transaction);
    }

    public async Task<TransactionDto?> UpdateAsync(Guid userId, Guid id, UpsertTransactionRequest request, CancellationToken cancellationToken)
    {
        var existing = await _gateway.GetByIdAsync(userId, id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.Date = request.Date;
        existing.Amount = Math.Abs(request.Amount);
        existing.Category = string.IsNullOrWhiteSpace(request.Category) ? "General" : request.Category.Trim();
        existing.Note = request.Note?.Trim();
        existing.Kind = request.Kind;
        existing.Status = request.Status;
        existing.UpdatedAt = DateTime.UtcNow;

        await _gateway.UpdateAsync(existing, cancellationToken);
        return Map(existing);
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken)
    {
        var existing = await _gateway.GetByIdAsync(userId, id, cancellationToken);
        if (existing is null)
        {
            return false;
        }

        await _gateway.DeleteAsync(existing, cancellationToken);
        return true;
    }

    private static TransactionDto Map(LedgerTransaction transaction) =>
        new(
            transaction.Id,
            transaction.Date,
            transaction.Amount,
            transaction.Category,
            transaction.Note,
            transaction.Kind,
            transaction.Status,
            transaction.RecurringRuleId,
            transaction.CreatedAt,
            transaction.UpdatedAt
        );
}
