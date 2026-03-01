namespace Backend.Application.Abstractions;

public interface ICurrentUserService
{
    Guid GetRequiredUserId();
    Guid? GetUserId();
}
