using System.Net;

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
/// Shared layout for account emails. The HTML uses inline styles and a table because many email
/// clients ignore style sheets. Every value is HTML-encoded, since the display name is user input.
/// </summary>
internal static class LinkEmailLayout
{
    public static EmailContent Render(LinkEmail email)
    {
        var greeting = string.IsNullOrWhiteSpace(email.DisplayName) ? "Hello," : $"Hello {email.DisplayName},";
        return new EmailContent(email.Subject, Html(email, greeting), Text(email, greeting));
    }

    private static string Html(LinkEmail email, string greeting)
    {
        static string E(string value) => WebUtility.HtmlEncode(value);

        return $"""
            <!doctype html>
            <html lang="en">
            <body style="margin:0;padding:24px;background:#f3f7f8;font-family:Arial,Helvetica,sans-serif;color:#1d2b33;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:8px;">
                <tr><td style="padding:32px;">
                  <p style="margin:0 0 8px;font-size:14px;font-weight:bold;color:#0f6e7a;">NCLEX Practice</p>
                  <h1 style="margin:0 0 16px;font-size:22px;">{E(email.Subject)}</h1>
                  <p style="margin:0 0 16px;font-size:16px;line-height:1.5;">{E(greeting)}</p>
                  <p style="margin:0 0 24px;font-size:16px;line-height:1.5;">{E(email.Intro)}</p>
                  <p style="margin:0 0 24px;">
                    <a href="{E(email.Url)}" style="display:inline-block;padding:12px 20px;background:#0f6e7a;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:bold;">{E(email.ButtonLabel)}</a>
                  </p>
                  <p style="margin:0 0 8px;font-size:14px;line-height:1.5;">If the button doesn't work, copy this link into your browser:<br><a href="{E(email.Url)}" style="color:#0f6e7a;word-break:break-all;">{E(email.Url)}</a></p>
                  <p style="margin:16px 0 0;font-size:14px;line-height:1.5;color:#4a5a63;">{E(email.ExpiryNote)} {E(email.IgnoreNote)}</p>
                </td></tr>
              </table>
            </body>
            </html>
            """;
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
