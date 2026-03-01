using Backend.Application.Models;

namespace Backend.Application.Services;

public interface IAccountService
{
    Task<AccountExportDto?> ExportAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> DeleteAccountAsync(Guid userId, CancellationToken cancellationToken);
}
