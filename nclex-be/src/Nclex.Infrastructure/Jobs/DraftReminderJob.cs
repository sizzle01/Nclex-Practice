using Microsoft.Extensions.Logging;
using Nclex.Application.Notifications.DraftReminders;

namespace Nclex.Infrastructure.Jobs;

/// <summary>The recurring "Pick up where you left off" run (architecture.md 5.3 and 5.6).</summary>
public sealed partial class DraftReminderJob(SendDraftRemindersHandler handler, ILogger<DraftReminderJob> logger)
{
    /// <summary>The recurring job's id in Hangfire.</summary>
    public const string RecurringJobId = "draft-reminders";

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var reminded = await handler.HandleAsync(cancellationToken);
        if (reminded > 0)
        {
            LogReminded(logger, reminded);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Sent {Count} draft reminders")]
    private static partial void LogReminded(ILogger logger, int count);
}
