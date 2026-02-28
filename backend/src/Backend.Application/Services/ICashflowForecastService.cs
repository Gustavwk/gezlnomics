using Backend.Application.Models;

namespace Backend.Application.Services;

public interface ICashflowForecastService
{
    Task SaveUserMonthAsync(Guid userId, int year, int month, UpsertUserMonthCashflowRequest request, CancellationToken cancellationToken);
    Task<CashflowForecastResult?> GetForecastAsync(Guid userId, int year, int month, CashflowForecastRequest request, CancellationToken cancellationToken);
}
