namespace Nclex.Domain.Security;

/// <summary>
/// The brute-force rule from architecture.md section 6: 5 failed attempts within 15 minutes lock
/// the account for a while. The same numbers apply to logins (counted per email and IP address)
/// and to re-entering the password in account settings (counted per user).
/// </summary>
public static class LoginLockoutPolicy
{
    /// <summary>Failed attempts allowed inside <see cref="Window"/>; the one that reaches it locks.</summary>
    public const int MaxFailures = 5;

    /// <summary>How far back failed attempts are counted (a sliding window).</summary>
    public static readonly TimeSpan Window = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How long a lock lasts. The design says "temporary" without a figure; matching the window
    /// means a locked attacker gets at most 5 guesses per 15 minutes either way.
    /// </summary>
    public static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(15);

    /// <summary>True when the failure just recorded, the <paramref name="failuresInWindow"/>th, should lock the account.</summary>
    public static bool ShouldLock(long failuresInWindow) => failuresInWindow >= MaxFailures;

    /// <summary>When a lock that starts at <paramref name="now"/> ends.</summary>
    public static DateTimeOffset LockedUntil(DateTimeOffset now) => now + LockDuration;
}
