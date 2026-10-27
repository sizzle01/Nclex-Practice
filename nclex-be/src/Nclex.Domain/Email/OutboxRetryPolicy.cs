namespace Nclex.Domain.Email;

/// <summary>
/// When to try a failed email again. Waits grow with each failure so a provider outage is not
/// hammered, and after <see cref="MaxAttempts"/> the email is marked failed.
/// </summary>
public static class OutboxRetryPolicy
{
    /// <summary>Total send attempts, including the first.</summary>
    public const int MaxAttempts = 5;

    // Wait after the 1st, 2nd, 3rd and 4th failure.
    private static readonly TimeSpan[] Delays =
    [
        TimeSpan.FromSeconds(30),
        TimeSpan.FromMinutes(2),
        TimeSpan.FromMinutes(10),
        TimeSpan.FromHours(1),
    ];

    /// <summary>
    /// The time of the next attempt after <paramref name="attemptsMade"/> failed attempts,
    /// or null if there should be no more attempts.
    /// </summary>
    public static DateTimeOffset? NextAttemptAt(int attemptsMade, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(attemptsMade, 1);
        return attemptsMade >= MaxAttempts ? null : now + Delays[attemptsMade - 1];
    }
}
