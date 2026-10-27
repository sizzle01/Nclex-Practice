namespace Nclex.Infrastructure.Email.Templates;

/// <summary>
/// The text of a security notice: what happened, and what to do if it was not the reader.
/// </summary>
/// <param name="Subject">Also used as the heading.</param>
/// <param name="DisplayName">Used in the greeting, if the user gave one.</param>
/// <param name="Paragraphs">What happened, one paragraph each.</param>
/// <param name="IfNotYou">What to do if the reader did not do this.</param>
/// <param name="ActionLabel">Label of the optional button, e.g. "Reset my password".</param>
/// <param name="ActionUrl">Where the button goes; no button when null.</param>
internal sealed record NoticeEmail(
    string Subject,
    string? DisplayName,
    IReadOnlyList<string> Paragraphs,
    string IfNotYou,
    string? ActionLabel = null,
    string? ActionUrl = null);

/// <summary>Layout for security notices. Every value is HTML-encoded (user agents and names are user input).</summary>
internal static class NoticeEmailLayout
{
    /// <summary>How times appear in notices. UTC, since payloads do not carry the user's time zone.</summary>
    public static string FormatTime(DateTimeOffset time) =>
        time.ToUniversalTime().ToString("d MMM yyyy 'at' HH:mm 'UTC'", System.Globalization.CultureInfo.InvariantCulture);

    public static EmailContent Render(NoticeEmail email)
    {
        var greeting = EmailFrame.Greeting(email.DisplayName);
        var paragraphs = string.Concat(email.Paragraphs.Select(EmailFrame.Paragraph));
        var button = email.ActionUrl is null ? string.Empty : EmailFrame.Button(email.ActionLabel!, email.ActionUrl);
        var html = EmailFrame.Html(
            email.Subject,
            greeting,
            $"""
            {paragraphs}
            {EmailFrame.Paragraph(email.IfNotYou)}
            {button}
            {EmailFrame.FinePrint("You're receiving this security notice because it concerns your NCLEX Practice account.")}
            """);
        return new EmailContent(email.Subject, html, Text(email, greeting));
    }

    private static string Text(NoticeEmail email, string greeting)
    {
        var action = email.ActionUrl is null ? string.Empty : $"{email.ActionLabel}: {email.ActionUrl}\n\n";
        return $"""
            {greeting}

            {string.Join("\n\n", email.Paragraphs)}

            {email.IfNotYou}

            {action}NCLEX Practice
            """;
    }
}
