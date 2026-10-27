using Nclex.Infrastructure.Email;

namespace Nclex.Worker;

/// <summary>
/// Polls the email outbox every <see cref="PollInterval"/> and sends due emails (architecture.md
/// section 4: Hangfire's one-minute minimum is too slow for sign-up emails, so this is a plain
/// BackgroundService). Each round gets a fresh DI scope, so each has its own DbContext.
/// </summary>
public sealed partial class EmailOutboxDispatcherService(
    IServiceScopeFactory scopeFactory,
    TimeProvider clock,
    ILogger<EmailOutboxDispatcherService> logger) : BackgroundService
{
    public static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    /// <summary>How often to look for emails whose worker stopped mid-send.</summary>
    public static readonly TimeSpan ReleaseInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval, clock);
        // MinValue: release any claims left by a previous run straight away.
        var lastRelease = DateTimeOffset.MinValue;

        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var dispatcher = scope.ServiceProvider.GetRequiredService<OutboxDispatcher>();

                if (clock.GetUtcNow() - lastRelease >= ReleaseInterval)
                {
                    await dispatcher.ReleaseExpiredClaimsAsync(stoppingToken);
                    lastRelease = clock.GetUtcNow();
                }

                await dispatcher.DispatchDueAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                // For example, the database is briefly unreachable. Keep polling; the next round retries.
                LogRoundFailed(logger, ex);
            }
        }
        while (await WaitForNextRoundAsync(timer, stoppingToken));
    }

    private static async Task<bool> WaitForNextRoundAsync(PeriodicTimer timer, CancellationToken stoppingToken)
    {
        try
        {
            return await timer.WaitForNextTickAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return false; // the host is shutting down
        }
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Email outbox round failed")]
    private static partial void LogRoundFailed(ILogger logger, Exception exception);
}
