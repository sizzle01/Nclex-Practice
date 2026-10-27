using System.Diagnostics;
using System.Threading.RateLimiting;
using Nclex.Application.Abstractions;

namespace Nclex.Api.RateLimiting;

/// <summary>
/// Plugs a Redis sliding window into ASP.NET Core's rate-limiting middleware, so the limit is
/// shared by every API instance rather than counted per process. One instance per partition
/// (for example per client IP); the middleware creates and caches them.
/// </summary>
internal sealed class RedisSlidingWindowLimiter(string key, int limit, TimeSpan window, IRateLimitStore store) : RateLimiter
{
    private long _lastUsed = Stopwatch.GetTimestamp();

    // Lets the middleware drop limiters for partitions (IPs) it has not seen for a while.
    public override TimeSpan? IdleDuration => Stopwatch.GetElapsedTime(Interlocked.Read(ref _lastUsed));

    public override RateLimiterStatistics? GetStatistics() => null;

    // The synchronous path cannot call Redis without blocking a thread. Saying "not acquired"
    // makes the middleware fall back to AcquireAsyncCore, which does the real check.
    protected override RateLimitLease AttemptAcquireCore(int permitCount) => WindowLease.NotChecked;

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        Interlocked.Exchange(ref _lastUsed, Stopwatch.GetTimestamp());
        var result = await store.TryHitAsync(key, limit, window, cancellationToken);
        return result.Allowed ? WindowLease.Acquired : WindowLease.Rejected(result.RetryAfter);
    }

    /// <summary>A lease that carries Retry-After when rejected.</summary>
    private sealed class WindowLease(bool isAcquired, TimeSpan? retryAfter) : RateLimitLease
    {
        public static readonly WindowLease Acquired = new(true, null);
        public static readonly WindowLease NotChecked = new(false, null);

        public static WindowLease Rejected(TimeSpan retryAfter) => new(false, retryAfter);

        public override bool IsAcquired { get; } = isAcquired;

        public override IEnumerable<string> MetadataNames =>
            retryAfter is null ? [] : [MetadataName.RetryAfter.Name];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            if (metadataName == MetadataName.RetryAfter.Name && retryAfter is not null)
            {
                metadata = retryAfter.Value;
                return true;
            }

            metadata = null;
            return false;
        }
    }
}
