using System.Collections.Concurrent;
using Backend.Application.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Backend.Infrastructure.Auth;

public sealed class LoginAttemptGuard : ILoginAttemptGuard
{
    private readonly int _maxFailedAttempts;
    private readonly TimeSpan _attemptWindow;
    private readonly TimeSpan _lockoutDuration;
    private readonly ConcurrentDictionary<string, AttemptState> _states = new();

    public LoginAttemptGuard(IConfiguration configuration)
    {
        _maxFailedAttempts = configuration.GetValue("LoginSecurity:MaxFailedAttempts", 5);
        _attemptWindow = TimeSpan.FromSeconds(configuration.GetValue("LoginSecurity:AttemptWindowSeconds", 600));
        _lockoutDuration = TimeSpan.FromSeconds(configuration.GetValue("LoginSecurity:LockoutDurationSeconds", 900));
    }

    public bool IsLocked(string username, string ipAddress, DateTime nowUtc, out DateTime lockedUntilUtc)
    {
        var key = BuildKey(username, ipAddress);
        if (!_states.TryGetValue(key, out var state))
        {
            lockedUntilUtc = DateTime.MinValue;
            return false;
        }

        lock (state)
        {
            if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value > nowUtc)
            {
                lockedUntilUtc = state.LockedUntilUtc.Value;
                return true;
            }

            if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value <= nowUtc)
            {
                state.Reset(nowUtc);
            }
        }

        lockedUntilUtc = DateTime.MinValue;
        return false;
    }

    public void RegisterFailure(string username, string ipAddress, DateTime nowUtc)
    {
        var key = BuildKey(username, ipAddress);
        var state = _states.GetOrAdd(key, _ => new AttemptState(nowUtc));

        lock (state)
        {
            if (nowUtc - state.FirstFailureUtc > _attemptWindow)
            {
                state.Reset(nowUtc);
            }

            state.FailureCount++;
            if (state.FailureCount >= _maxFailedAttempts)
            {
                state.LockedUntilUtc = nowUtc.Add(_lockoutDuration);
            }
        }
    }

    public void RegisterSuccess(string username, string ipAddress)
    {
        var key = BuildKey(username, ipAddress);
        _states.TryRemove(key, out _);
    }

    private static string BuildKey(string username, string ipAddress)
        => $"{username.Trim().ToLowerInvariant()}|{ipAddress}";

    private sealed class AttemptState
    {
        public AttemptState(DateTime nowUtc)
        {
            FirstFailureUtc = nowUtc;
        }

        public DateTime FirstFailureUtc { get; private set; }
        public int FailureCount { get; set; }
        public DateTime? LockedUntilUtc { get; set; }

        public void Reset(DateTime nowUtc)
        {
            FirstFailureUtc = nowUtc;
            FailureCount = 0;
            LockedUntilUtc = null;
        }
    }
}
