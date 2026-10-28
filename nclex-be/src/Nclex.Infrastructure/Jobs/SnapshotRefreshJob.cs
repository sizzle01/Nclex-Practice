using Microsoft.Extensions.Logging;
using Nclex.Application.Performance.Snapshots;

namespace Nclex.Infrastructure.Jobs;

/// <summary>
/// Queued after every submit commits (architecture.md 5.5). Hangfire may run it more than once or
/// out of order; the handler makes both harmless.
/// </summary>
public sealed partial class SnapshotRefreshJob(RefreshSnapshotsHandler handler, ILogger<SnapshotRefreshJob> logger)
{
    public async Task RunAsync(Guid userId, Guid attemptId, CancellationToken cancellationToken)
    {
        var added = await handler.HandleAsync(userId, attemptId, cancellationToken);
        LogRefreshed(logger, attemptId, added);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Performance snapshots refreshed for attempt {AttemptId}: {Added} attempts added")]
    private static partial void LogRefreshed(ILogger logger, Guid attemptId, int added);
}
