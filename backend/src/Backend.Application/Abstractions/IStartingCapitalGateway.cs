using Backend.Domain;

namespace Backend.Application.Abstractions;

public interface IStartingCapitalGateway
{
    Task<StartingCapital?> GetLatestAsync(CancellationToken cancellationToken);
    Task AddAsync(StartingCapital startingCapital, CancellationToken cancellationToken);
}
