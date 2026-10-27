namespace Nclex.Infrastructure.Email.Templates;

/// <summary>The text of an email whose main point is one button with a link.</summary>
internal sealed record LinkEmail(
    string Subject,
    string? DisplayName,
    string Intro,
    string ButtonLabel,
    string Url,
    string ExpiryNote,
    string IgnoreNote);

/// <summary>
/// Layout for emails built around one link (verify, reset, confirm a new address). Every value is
/// HTML-encoded, since the display name is user input.
/// </summary>
internal static class LinkEmailLayout
{
    public static EmailContent Render(LinkEmail email)
    {
        var greeting = EmailFrame.Greeting(email.DisplayName);
        var html = EmailFrame.Html(
            email.Subject,
            greeting,
            $"""
            <p style="margin:0 0 24px;font-size:16px;line-height:1.5;">{EmailFrame.E(email.Intro)}</p>
            {EmailFrame.Button(email.ButtonLabel, email.Url)}
            {EmailFrame.FinePrint($"{email.ExpiryNote} {email.IgnoreNote}")}
            """);
        return new EmailContent(email.Subject, html, Text(email, greeting));
    }

    private static string Text(LinkEmail email, string greeting) =>
        $"""
        {greeting}

        {email.Intro}

        {email.ButtonLabel}: {email.Url}

        {email.ExpiryNote} {email.IgnoreNote}

        NCLEX Practice
        """;
}
