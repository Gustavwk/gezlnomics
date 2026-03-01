using Backend.Application.Models;

namespace Backend.Application.Services;

public interface IIncomePeriodService
{
    Task<IReadOnlyList<IncomePeriodDto>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
    Task<IncomePeriodDto> CreateAsync(Guid userId, UpsertIncomePeriodRequest request, CancellationToken cancellationToken);
    Task<IncomePeriodDto?> UpdateAsync(Guid userId, Guid id, UpsertIncomePeriodRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
