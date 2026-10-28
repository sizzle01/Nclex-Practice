using Nclex.Application.Email;
using Nclex.Domain.Notifications;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>"Pick up where you left off", for users who turned on reminder emails.</summary>
internal sealed class DraftReminderTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.DraftReminder;

    public bool PayloadHoldsSecret => false;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<DraftReminderEmailPayload>(payload);
        var text = NotificationCopy.DraftReminder(values.SetTitle, values.AnsweredShare, values.TimeLeftMs);
        return NotificationEmailLayout.Render(new NotificationEmailText(
            Subject: text.Title,
            DisplayName: values.DisplayName,
            Paragraphs: [text.Body, "Your answers are saved, so you can carry on right where you stopped."],
            ActionLabel: "Resume my test",
            ActionUrl: links.App(values.AttemptPath),
            SettingsUrl: links.NotificationSettings()));
    }
}
