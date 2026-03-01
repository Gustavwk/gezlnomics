using Backend.Application.Abstractions;
using Backend.Application.Models;
using Backend.Domain;

namespace Backend.Application.Services;

public sealed class IncomePeriodService : IIncomePeriodService
{
    private readonly IIncomePeriodGateway _gateway;

    public IncomePeriodService(IIncomePeriodGateway gateway)
    {
        _gateway = gateway;
    }

    public async Task<IReadOnlyList<IncomePeriodDto>> GetAllAsync(Guid userId, DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var periods = await _gateway.GetAllAsync(userId, from, to, cancellationToken);
        return periods.Select(Map).ToList();
    }

    public async Task<IncomePeriodDto> CreateAsync(Guid userId, UpsertIncomePeriodRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var period = new IncomePeriod
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PeriodStartDate = request.PeriodStartDate,
            PeriodEndDate = request.PeriodEndDate,
            StartingBalance = request.StartingBalance,
            CreatedAt = DateTime.UtcNow
        };

        await _gateway.AddAsync(period, cancellationToken);
        return Map(period);
    }

    public async Task<IncomePeriodDto?> UpdateAsync(Guid userId, Guid id, UpsertIncomePeriodRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var existing = await _gateway.GetByIdAsync(userId, id, cancellationToken);
        if (existing is null)
        {
            return null;
        }

        existing.PeriodStartDate = request.PeriodStartDate;
        existing.PeriodEndDate = request.PeriodEndDate;
        existing.StartingBalance = request.StartingBalance;
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

    private static void Validate(UpsertIncomePeriodRequest request)
    {
        if (request.PeriodEndDate < request.PeriodStartDate)
        {
            throw new InvalidOperationException("PeriodEndDate skal være lig med eller efter PeriodStartDate.");
        }
    }

    private static IncomePeriodDto Map(IncomePeriod period) =>
        new(period.Id, period.PeriodStartDate, period.PeriodEndDate, period.StartingBalance, period.CreatedAt);
}
