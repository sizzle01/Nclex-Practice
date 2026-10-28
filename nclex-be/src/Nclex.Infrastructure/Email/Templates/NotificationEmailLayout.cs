namespace Nclex.Infrastructure.Email.Templates;

/// <summary>The text of a notification email: what happened and where to go next.</summary>
/// <param name="Subject">Also used as the heading; the notification's title.</param>
/// <param name="DisplayName">Used in the greeting, if the user gave one.</param>
/// <param name="Paragraphs">The message, one paragraph each.</param>
/// <param name="ActionLabel">Label of the button, e.g. "See my results".</param>
/// <param name="ActionUrl">Where the button goes.</param>
/// <param name="SettingsUrl">The notification settings page, offered to opt out.</param>
internal sealed record NotificationEmailText(
    string Subject,
    string? DisplayName,
    IReadOnlyList<string> Paragraphs,
    string ActionLabel,
    string ActionUrl,
    string SettingsUrl);

/// <summary>
/// Layout for notification emails (results, reminders). Unlike security notices, the user chose to
/// get these, so each ends with where to switch them off. Every value is HTML-encoded.
/// </summary>
internal static class NotificationEmailLayout
{
    private const string OptOut = "You're receiving this because email is on for this kind of notification. You can change that in your notification settings:";

    public static EmailContent Render(NotificationEmailText email)
    {
        var greeting = EmailFrame.Greeting(email.DisplayName);
        var html = EmailFrame.Html(
            email.Subject,
            greeting,
            $"""
            {string.Concat(email.Paragraphs.Select(EmailFrame.Paragraph))}
            {EmailFrame.Button(email.ActionLabel, email.ActionUrl)}
            {EmailFrame.FinePrint($"{OptOut} {email.SettingsUrl}")}
            """);
        var text = $"""
            {greeting}

            {string.Join("\n\n", email.Paragraphs)}

            {email.ActionLabel}: {email.ActionUrl}

            {OptOut} {email.SettingsUrl}

            NCLEX Practice
            """;
        return new EmailContent(email.Subject, html, text);
    }
}
