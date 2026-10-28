using Nclex.Application.Email;
using Nclex.Domain.Notifications;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>
/// The three result emails share a payload and a layout; they differ only in wording, which
/// comes from <see cref="NotificationCopy"/> so the email says what the bell says.
/// </summary>
internal abstract class ResultEmailTemplate(EmailLinks links) : IEmailTemplate
{
    public abstract string Name { get; }

    public bool PayloadHoldsSecret => false;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<ResultEmailPayload>(payload);
        var text = Text(values);
        return NotificationEmailLayout.Render(new NotificationEmailText(
            Subject: text.Title,
            DisplayName: values.DisplayName,
            Paragraphs: [text.Body, "Your results page has the full breakdown by category, and every rationale is ready to review."],
            ActionLabel: "See my results",
            ActionUrl: links.App(values.ResultsPath),
            SettingsUrl: links.NotificationSettings()));
    }

    protected abstract NotificationText Text(ResultEmailPayload values);
}

internal sealed class ResultPassTemplate(EmailLinks links) : ResultEmailTemplate(links)
{
    public override string Name => EmailTemplateNames.ResultPass;

    protected override NotificationText Text(ResultEmailPayload values) =>
        NotificationCopy.ResultPass(values.ScorePct, values.SetTitle);
}

internal sealed class ResultFailTemplate(EmailLinks links) : ResultEmailTemplate(links)
{
    public override string Name => EmailTemplateNames.ResultFail;

    protected override NotificationText Text(ResultEmailPayload values) =>
        NotificationCopy.ResultFail(values.ScorePct, values.Weakest);
}

internal sealed class ResultReadyTemplate(EmailLinks links) : ResultEmailTemplate(links)
{
    public override string Name => EmailTemplateNames.ResultReady;

    protected override NotificationText Text(ResultEmailPayload values) =>
        NotificationCopy.ResultReady(values.ScorePct, values.SetTitle);
}
