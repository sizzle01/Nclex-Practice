using Microsoft.Extensions.Logging;
using Nclex.Application.Attempts.Expiry;

namespace Nclex.Infrastructure.Jobs;

/// <summary>The recurring safety net behind the delayed jobs (architecture.md 5.1, rule 3).</summary>
public sealed partial class ExpirySweeperJob(SweepExpiredAttemptsHandler handler, ILogger<ExpirySweeperJob> logger)
{
    /// <summary>The recurring job's id in Hangfire.</summary>
    public const string RecurringJobId = "attempt-expiry-sweeper";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var finalized = await handler.HandleAsync(cancellationToken);
        if (finalized > 0)
        {
            // Each of these should have been caught by its delayed job; a steady count means jobs are being lost.
            LogSwept(logger, finalized);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Expiry sweeper auto-submitted {Count} attempts")]
    private static partial void LogSwept(ILogger logger, int count);
}
