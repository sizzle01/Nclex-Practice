using Nclex.Application.Abstractions;
using Nclex.Application.Email;
using Nclex.Domain.Flags;
using Nclex.Domain.Notifications;

namespace Nclex.Application.Notifications;

/// <summary>
/// The notification a reporter gets when a moderator resolves their report (interfaces.md 5.6,
/// <c>flag_resolved</c>). It is in the content category: in-app by default, by email only if the
/// user turned that on.
/// </summary>
public static class FlagNotifications
{
    /// <summary>The "My reports" page, where the reporter sees every report and its status.</summary>
    public const string DeepLink = "/settings/reports";

    public static NewNotification Resolved(QuestionFlag flag, string? displayName)
    {
        var note = flag.ResolutionNote
            ?? throw new InvalidOperationException("Only a resolved report, which has a note, notifies its reporter.");
        return new NewNotification(
            flag.UserId,
            NotificationType.FlagResolved,
            NotificationCategory.Content,
            NotificationCopy.FlagResolved(note),
            new { flagId = flag.Id, questionVersionId = flag.QuestionVersionId, deepLink = DeepLink },
            RelatedAttemptId: flag.AttemptId,
            new NotificationEmail(EmailTemplateNames.FlagResolved, new FlagResolvedEmailPayload(displayName, note, DeepLink)));
    }
}
