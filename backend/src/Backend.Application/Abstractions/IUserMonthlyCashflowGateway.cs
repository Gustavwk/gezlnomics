using Backend.Domain;

namespace Backend.Application.Abstractions;

public interface IUserMonthlyCashflowGateway
{
    Task<UserMonthlyCashflow?> GetByUserMonthAsync(Guid userId, int year, int month, CancellationToken cancellationToken);
    Task<IReadOnlyList<UserMonthlyCashflow>> GetByUserInPeriodAsync(Guid userId, DateOnly start, DateOnly end, CancellationToken cancellationToken);
    Task SaveAsync(UserMonthlyCashflow cashflow, CancellationToken cancellationToken);
}
