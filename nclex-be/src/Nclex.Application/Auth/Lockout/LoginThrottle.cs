using System.Net;
using System.Security.Cryptography;
using System.Text;
using Nclex.Application.Abstractions;
using Nclex.Domain.Security;

namespace Nclex.Application.Auth.Lockout;

/// <summary>
/// Counts failed password checks in Redis (architecture.md section 6, "Brute force"): logins per
/// (email, IP address), and re-entered passwords in account settings per user. Each count is a
/// 15-minute sliding window; <see cref="LoginLockoutPolicy"/> decides when it locks the account.
/// </summary>
public sealed class LoginThrottle(IRateLimitStore store)
{
    /// <summary>Records a failed login and returns how many there have been in the window, this one included.</summary>
    public async Task<long> RecordLoginFailureAsync(string email, IPAddress? ip, CancellationToken cancellationToken) =>
        (await store.TryHitAsync(LoginKey(email, ip), int.MaxValue, LoginLockoutPolicy.Window, cancellationToken)).Count;

    /// <summary>
    /// For an email with no account: how long to answer as if it were locked, or null. Unknown
    /// emails must look exactly like real ones, or the lock message would reveal which emails exist.
    /// </summary>
    public async Task<TimeSpan?> UnknownEmailLockAsync(string email, IPAddress? ip, CancellationToken cancellationToken)
    {
        var window = await store.PeekAsync(LoginKey(email, ip), LoginLockoutPolicy.MaxFailures, LoginLockoutPolicy.Window, cancellationToken);
        return window.Allowed ? null : window.RetryAfter;
    }

    /// <summary>Starts the count again, after a successful login or once the account is locked.</summary>
    public Task ResetLoginFailuresAsync(string email, IPAddress? ip, CancellationToken cancellationToken) =>
        store.ResetAsync(LoginKey(email, ip), cancellationToken);

    /// <summary>Records a wrong current password in account settings and returns the count in the window.</summary>
    public async Task<long> RecordReauthFailureAsync(Guid userId, CancellationToken cancellationToken) =>
        (await store.TryHitAsync(ReauthKey(userId), int.MaxValue, LoginLockoutPolicy.Window, cancellationToken)).Count;

    public Task ResetReauthFailuresAsync(Guid userId, CancellationToken cancellationToken) =>
        store.ResetAsync(ReauthKey(userId), cancellationToken);

    // The email is hashed so Redis never holds addresses (personal data). Lower-cased first because
    // the email column is citext: "Ada@x.com" and "ada@x.com" are the same account.
    private static string LoginKey(string email, IPAddress? ip)
    {
        var emailHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant())));
        return $"login-fail:{emailHash}:{ip?.ToString() ?? "unknown"}";
    }

    private static string ReauthKey(Guid userId) => $"reauth-fail:{userId}";
}
