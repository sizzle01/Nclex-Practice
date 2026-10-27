using System.Net;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>
/// The outer HTML shared by every account email: brand line, heading, greeting, then the
/// email's own content. Inline styles and a table, because many email clients ignore style sheets.
/// </summary>
internal static class EmailFrame
{
    /// <summary>"Hello Ada," or "Hello," when there is no display name.</summary>
    public static string Greeting(string? displayName) =>
        string.IsNullOrWhiteSpace(displayName) ? "Hello," : $"Hello {displayName},";

    /// <summary>HTML-encodes a value; every user-supplied value must go through this.</summary>
    public static string E(string value) => WebUtility.HtmlEncode(value);

    /// <summary>A body paragraph, encoded.</summary>
    public static string Paragraph(string text) =>
        $"""<p style="margin:0 0 16px;font-size:16px;line-height:1.5;">{E(text)}</p>""";

    /// <summary>The main button, with the link repeated as text for clients that hide buttons.</summary>
    public static string Button(string label, string url) =>
        $"""
        <p style="margin:0 0 24px;">
          <a href="{E(url)}" style="display:inline-block;padding:12px 20px;background:#0f6e7a;color:#ffffff;text-decoration:none;border-radius:6px;font-weight:bold;">{E(label)}</a>
        </p>
        <p style="margin:0 0 8px;font-size:14px;line-height:1.5;">If the button doesn't work, copy this link into your browser:<br><a href="{E(url)}" style="color:#0f6e7a;word-break:break-all;">{E(url)}</a></p>
        """;

    /// <summary>Small grey print at the end, encoded.</summary>
    public static string FinePrint(string text) =>
        $"""<p style="margin:16px 0 0;font-size:14px;line-height:1.5;color:#4a5a63;">{E(text)}</p>""";

    public static string Html(string subject, string greeting, string contentHtml) =>
        $"""
        <!doctype html>
        <html lang="en">
        <body style="margin:0;padding:24px;background:#f3f7f8;font-family:Arial,Helvetica,sans-serif;color:#1d2b33;">
          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" style="max-width:560px;margin:0 auto;background:#ffffff;border-radius:8px;">
            <tr><td style="padding:32px;">
              <p style="margin:0 0 8px;font-size:14px;font-weight:bold;color:#0f6e7a;">NCLEX Practice</p>
              <h1 style="margin:0 0 16px;font-size:22px;">{E(subject)}</h1>
              <p style="margin:0 0 16px;font-size:16px;line-height:1.5;">{E(greeting)}</p>
              {contentHtml}
            </td></tr>
          </table>
        </body>
        </html>
        """;
}
