using Nclex.Application.Abstractions;
using Nclex.Application.Email;
using Nclex.Domain.Entities;
using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications;

/// <summary>One of the weakest categories of a result: its key for the client, its name for people.</summary>
public sealed record WeakCategory(string Key, string Name);

/// <summary>
/// The notification grading creates when an attempt is submitted (architecture.md 5.5): pass, fail,
/// or just "ready" when the set has no pass threshold.
/// </summary>
public static class ResultNotifications
{
    /// <summary>The results page, opened at the category breakdown (interfaces.md 5.5).</summary>
    public static string DeepLink(Guid attemptId) => $"/attempts/{attemptId}/results#breakdown";

    /// <param name="weakest">The weakest categories, worst first.</param>
    public static NewNotification For(
        Attempt attempt,
        AttemptResult result,
        string setTitle,
        string? displayName,
        IReadOnlyList<WeakCategory> weakest)
    {
        var (type, template) = result.Passed switch
        {
            true => (NotificationType.ResultPass, EmailTemplateNames.ResultPass),
            false => (NotificationType.ResultFail, EmailTemplateNames.ResultFail),
            null => (NotificationType.ResultReady, EmailTemplateNames.ResultReady),
        };
        var names = weakest.Select(w => w.Name).ToList();
        var text = type switch
        {
            NotificationType.ResultPass => NotificationCopy.ResultPass(result.ScorePct, setTitle),
            NotificationType.ResultFail => NotificationCopy.ResultFail(result.ScorePct, names),
            _ => NotificationCopy.ResultReady(result.ScorePct, setTitle),
        };
        var deepLink = DeepLink(attempt.Id);

        return new NewNotification(
            attempt.UserId,
            type,
            NotificationCategory.Results,
            text,
            new { scorePct = result.ScorePct, weakest = weakest.Select(w => w.Key).ToList(), deepLink },
            attempt.Id,
            new NotificationEmail(template, new ResultEmailPayload(displayName, setTitle, result.ScorePct, names, deepLink)));
    }
}
