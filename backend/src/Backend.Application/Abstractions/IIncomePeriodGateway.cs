using Backend.Domain;

namespace Backend.Application.Abstractions;

public interface IIncomePeriodGateway
{
    Task<IReadOnlyList<IncomePeriod>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
    Task<IncomePeriod?> GetByIdAsync(Guid userId, Guid id, CancellationToken cancellationToken);
    Task<IncomePeriod?> GetForPeriodAsync(Guid userId, DateOnly periodStart, DateOnly periodEnd, CancellationToken cancellationToken);
    Task AddAsync(IncomePeriod period, CancellationToken cancellationToken);
    Task UpdateAsync(IncomePeriod period, CancellationToken cancellationToken);
    Task DeleteAsync(IncomePeriod period, CancellationToken cancellationToken);
    Task DeleteAllByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
