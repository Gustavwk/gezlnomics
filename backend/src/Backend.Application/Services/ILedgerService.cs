using Backend.Application.Models;

namespace Backend.Application.Services;

public interface ILedgerService
{
    Task<LedgerSummaryDto> GetSummaryAsync(Guid userId, DateOnly asOf, CancellationToken cancellationToken);
    Task<IReadOnlyList<LedgerTimelinePointDto>> GetTimelineAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken);
}
