using Microsoft.Extensions.Logging;
using Nclex.Application.Abstractions;
using StackExchange.Redis;

namespace Nclex.Infrastructure.RateLimiting;

/// <summary>
/// Sliding windows kept as Redis sorted sets: one member per hit, scored with the hit's time in
/// milliseconds. Each call runs as a Lua script, so trimming, counting and adding are one atomic
/// step even when many API instances hit the same key at once.
/// </summary>
/// <remarks>
/// Times come from <see cref="TimeProvider"/>, not from the Redis server's clock, so tests can
/// move time forward with a fake clock.
/// </remarks>
internal sealed partial class RedisRateLimitStore(
    IConnectionMultiplexer redis,
    TimeProvider clock,
    ILogger<RedisRateLimitStore> logger) : IRateLimitStore
{
    private const string KeyPrefix = "ratelimit:";

    // KEYS[1] = window key. ARGV = now (ms), window (ms), limit, member, record (1 or 0).
    // Returns { allowed (1/0), count, oldest score or -1 }.
    private static readonly LuaScript HitScript = LuaScript.Prepare("""
        local now = tonumber(@now)
        local window = tonumber(@window)
        redis.call('ZREMRANGEBYSCORE', @key, '-inf', now - window)
        local count = redis.call('ZCARD', @key)
        local allowed = 0
        if count < tonumber(@limit) then
          allowed = 1
          if @record == '1' then
            redis.call('ZADD', @key, now, @member)
            redis.call('PEXPIRE', @key, window)
            count = count + 1
          end
        end
        local oldest = redis.call('ZRANGE', @key, 0, 0, 'WITHSCORES')
        local oldestScore = -1
        if oldest[2] then oldestScore = tonumber(oldest[2]) end
        return { allowed, count, oldestScore }
        """);

    public Task<SlidingWindow> TryHitAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken) =>
        RunAsync(key, limit, window, record: true);

    public Task<SlidingWindow> PeekAsync(string key, int limit, TimeSpan window, CancellationToken cancellationToken) =>
        RunAsync(key, limit, window, record: false);

    public async Task ResetAsync(string key, CancellationToken cancellationToken)
    {
        if (!redis.IsConnected)
        {
            LogRedisUnavailable(logger);
            return;
        }

        try
        {
            await redis.GetDatabase().KeyDeleteAsync(KeyPrefix + key);
        }
        catch (RedisException ex)
        {
            LogRedisFailed(logger, ex);
        }
    }

    private async Task<SlidingWindow> RunAsync(string key, int limit, TimeSpan window, bool record)
    {
        // Checked first so an outage costs nothing per request, instead of a timeout each time.
        if (!redis.IsConnected)
        {
            LogRedisUnavailable(logger);
            return SlidingWindow.Unavailable;
        }

        var nowMs = clock.GetUtcNow().ToUnixTimeMilliseconds();
        var windowMs = (long)window.TotalMilliseconds;
        try
        {
            var result = (RedisResult[])(await redis.GetDatabase().ScriptEvaluateAsync(HitScript, new
            {
                key = (RedisKey)(KeyPrefix + key),
                now = nowMs,
                window = windowMs,
                limit,
                // Unique per hit: two hits in the same millisecond must both count.
                member = $"{nowMs}:{Guid.NewGuid():N}",
                record = record ? "1" : "0",
            }))!;

            var allowed = (long)result[0] == 1;
            var count = (long)result[1];
            var oldestMs = (long)result[2];
            var retryAfter = allowed || oldestMs < 0
                ? TimeSpan.Zero
                : TimeSpan.FromMilliseconds(oldestMs + windowMs - nowMs);
            return new SlidingWindow(allowed, count, retryAfter);
        }
        catch (RedisException ex)
        {
            LogRedisFailed(logger, ex);
            return SlidingWindow.Unavailable;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis is not connected; rate limits are not being enforced")]
    private static partial void LogRedisUnavailable(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis rate-limit call failed; allowing the request")]
    private static partial void LogRedisFailed(ILogger logger, Exception exception);
}
