using Nclex.Application.Email;
using Nclex.Domain.Notifications;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>"Thanks — we reviewed your report", for users who turned on content emails.</summary>
internal sealed class FlagResolvedTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.FlagResolved;

    public bool PayloadHoldsSecret => false;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<FlagResolvedEmailPayload>(payload);
        var text = NotificationCopy.FlagResolved(values.ResolutionNote);
        return NotificationEmailLayout.Render(new NotificationEmailText(
            Subject: text.Title,
            DisplayName: values.DisplayName,
            Paragraphs: ["Thank you for reporting a problem with a question. Here is what we found:", text.Body],
            ActionLabel: "See my reports",
            ActionUrl: links.App(values.ReportsPath),
            SettingsUrl: links.NotificationSettings()));
    }
}
