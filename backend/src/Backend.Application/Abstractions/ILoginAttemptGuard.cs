namespace Backend.Application.Abstractions;

public interface ILoginAttemptGuard
{
    bool IsLocked(string username, string ipAddress, DateTime nowUtc, out DateTime lockedUntilUtc);
    void RegisterFailure(string username, string ipAddress, DateTime nowUtc);
    void RegisterSuccess(string username, string ipAddress);
}
