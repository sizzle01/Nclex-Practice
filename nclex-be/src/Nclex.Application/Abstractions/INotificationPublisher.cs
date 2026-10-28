namespace Nclex.Application.Abstractions;

/// <summary>
/// Creates in-app notifications and, per the user's preferences, their emails (interfaces.md
/// section 3, architecture.md 5.6).
/// </summary>
public interface INotificationPublisher
{
    /// <summary>
    /// Adds the notification and its email_outbox row to the current DbContext, so they commit in the
    /// caller's transaction. Publishes to Redis for the bell after the commit.
    /// </summary>
    Task PublishAsync(NewNotification notification, CancellationToken cancellationToken);
}

/// <summary>Which template a notification uses (database enum <c>notification_type</c>, interfaces.md 5.6).</summary>
public enum NotificationType
{
    /// <summary>The score reached the set's pass threshold.</summary>
    ResultPass,

    /// <summary>The score fell short of the set's pass threshold.</summary>
    ResultFail,

    /// <summary>The set has no pass threshold.</summary>
    ResultReady,
}

/// <summary>A result notification, with the values its template needs (the <c>data</c> column).</summary>
/// <param name="ScorePct">The score, 0 to 1.</param>
/// <param name="WeakestCategories">Names of the weakest Client Needs categories, worst first.</param>
/// <param name="DeepLink">The page the notification opens, e.g. "/attempts/{id}/results".</param>
public sealed record NewNotification(
    Guid UserId,
    NotificationType Type,
    Guid RelatedAttemptId,
    string SetTitle,
    decimal ScorePct,
    IReadOnlyList<string> WeakestCategories,
    string DeepLink);
