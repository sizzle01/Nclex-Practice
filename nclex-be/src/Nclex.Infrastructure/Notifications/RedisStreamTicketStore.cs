using Microsoft.Extensions.Logging;
using Nclex.Application.Abstractions;
using StackExchange.Redis;

namespace Nclex.Infrastructure.Notifications;

/// <summary>
/// Stream tickets kept in Redis with a time to live. Only the ticket's hash is stored, as with
/// refresh tokens, and GETDEL makes each ticket work exactly once, whichever replica redeems it.
/// </summary>
internal sealed partial class RedisStreamTicketStore(
    IConnectionMultiplexer redis,
    ISecureTokenGenerator tokens,
    ILogger<RedisStreamTicketStore> logger) : IStreamTicketStore
{
    private const string KeyPrefix = "notif-ticket:";

    public async Task<string?> IssueAsync(Guid userId, TimeSpan lifetime, CancellationToken cancellationToken)
    {
        if (!redis.IsConnected)
        {
            LogRedisUnavailable(logger);
            return null;
        }

        var ticket = tokens.Generate();
        try
        {
            await redis.GetDatabase().StringSetAsync(Key(ticket.Hash), userId.ToString(), lifetime);
            return ticket.Value;
        }
        catch (RedisException ex)
        {
            LogRedisFailed(logger, ex);
            return null;
        }
    }

    public async Task<Guid?> RedeemAsync(string ticket, CancellationToken cancellationToken)
    {
        if (!redis.IsConnected)
        {
            LogRedisUnavailable(logger);
            return null;
        }

        try
        {
            var userId = await redis.GetDatabase().StringGetDeleteAsync(Key(tokens.Hash(ticket)));
            return Guid.TryParse(userId.ToString(), out var id) ? id : null;
        }
        catch (RedisException ex)
        {
            LogRedisFailed(logger, ex);
            return null;
        }
    }

    private static RedisKey Key(byte[] hash) => KeyPrefix + Convert.ToHexString(hash);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Redis is unavailable; no notification stream can be opened")]
    private static partial void LogRedisUnavailable(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "A stream ticket call to Redis failed")]
    private static partial void LogRedisFailed(ILogger logger, Exception exception);
}
