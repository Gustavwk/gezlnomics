using Backend.Application.Models;

namespace Backend.Application.Services;

public interface IRecurringRuleService
{
    Task<IReadOnlyList<RecurringRuleDto>> GetAllAsync(Guid userId, CancellationToken cancellationToken);
    Task<RecurringRuleDto> CreateAsync(Guid userId, UpsertRecurringRuleRequest request, CancellationToken cancellationToken);
    Task<RecurringRuleDto?> UpdateAsync(Guid userId, Guid id, UpsertRecurringRuleRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid userId, Guid id, CancellationToken cancellationToken);
}
