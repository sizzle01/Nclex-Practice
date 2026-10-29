using Microsoft.Extensions.Logging;
using Nclex.Application.Performance.Snapshots;

namespace Nclex.Infrastructure.Jobs;

/// <summary>Queued by an admin through POST /admin/performance-snapshots/rebuild.</summary>
public sealed partial class SnapshotRebuildJob(RebuildSnapshotsHandler handler, ILogger<SnapshotRebuildJob> logger)
{
    public async Task RunAsync(Guid userId, CancellationToken cancellationToken)
    {
        var attempts = await handler.HandleAsync(userId, cancellationToken);
        LogRebuilt(logger, userId, attempts);
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Performance snapshots of user {UserId} rebuilt from {Attempts} attempts")]
    private static partial void LogRebuilt(ILogger logger, Guid userId, int attempts);
}
