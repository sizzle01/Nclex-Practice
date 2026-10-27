namespace Nclex.Application.Abstractions;

/// <summary>
/// Sliding-window counters shared by every API instance (Redis, architecture.md section 6,
/// "Brute force"). A window counts the hits recorded under a key in the last <c>window</c> of time.
/// </summary>
/// <remarks>
/// Redis only holds rebuildable data, so when it is unreachable these methods fail open: they
/// report an empty window rather than throw, and rate limiting is degraded, not the API
/// (architecture.md section 7, "Availability").
/// </remarks>
public interface IRateLimitStore
{
    /// <summary>
    /// Records a hit under <paramref name="key"/> unless the window already holds
    /// <paramref name="limit"/> hits. Returns the window as it is afterwards.
    /// </summary>
    Task<SlidingWindow> TryHitAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken);

    /// <summary>Reads the window without recording anything.</summary>
    Task<SlidingWindow> PeekAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken);

    /// <summary>Forgets every hit under <paramref name="key"/>.</summary>
    Task ResetAsync(string key, CancellationToken cancellationToken);
}

/// <summary>The state of one key's window.</summary>
/// <param name="Allowed">False when the window was already full, so nothing was recorded.</param>
/// <param name="Count">Hits now in the window.</param>
/// <param name="RetryAfter">When full: how long until the oldest hit leaves the window. Otherwise zero.</param>
public sealed record SlidingWindow(bool Allowed, long Count, TimeSpan RetryAfter)
{
    /// <summary>What the store reports when it cannot reach Redis.</summary>
    public static readonly SlidingWindow Unavailable = new(Allowed: true, Count: 0, RetryAfter: TimeSpan.Zero);
}
