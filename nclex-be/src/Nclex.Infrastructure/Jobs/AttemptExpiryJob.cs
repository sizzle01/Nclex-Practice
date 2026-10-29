using Microsoft.Extensions.Logging;
using Nclex.Application.Attempts.Expiry;

namespace Nclex.Infrastructure.Jobs;

/// <summary>
/// The delayed job scheduled whenever an exam timer starts or resumes (architecture.md 5.1, rule 2).
/// Hangfire stores the attempt id and timer epoch, and passes its own cancellation token.
/// </summary>
public sealed partial class AttemptExpiryJob(ExpireAttemptHandler handler, ILogger<AttemptExpiryJob> logger)
{
    public async Task RunAsync(Guid attemptId, int timerEpoch, CancellationToken cancellationToken)
    {
        // Every log line from this run, including any failure, carries the attemptId (architecture.md section 9).
        using var scope = logger.BeginScope(new Dictionary<string, object> { ["AttemptId"] = attemptId });
        if (await handler.HandleAsync(attemptId, timerEpoch, cancellationToken))
        {
            LogExpired(logger, attemptId);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Attempt {AttemptId} auto-submitted: exam time expired")]
    private static partial void LogExpired(ILogger logger, Guid attemptId);
}
