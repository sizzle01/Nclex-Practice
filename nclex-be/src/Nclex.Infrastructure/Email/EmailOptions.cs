using System.Net;

namespace Nclex.Infrastructure.Email;

/// <summary>Settings for sending email, bound from the "Email" configuration section.</summary>
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Sender shown to recipients, e.g. "NCLEX Practice &lt;no-reply@example.com&gt;".</summary>
    public string From { get; set; } = string.Empty;

    /// <summary>Where the Next.js app is served; emailed links point at its pages.</summary>
    public string FrontendBaseUrl { get; set; } = string.Empty;

    public SmtpSettings Smtp { get; set; } = new();
}

/// <summary>SMTP server to hand emails to. Locally this is Mailpit (docker-compose.yml).</summary>
public sealed class SmtpSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 25;

    public bool EnableSsl { get; set; }

    /// <summary>
    /// Login for providers that require authentication (Brevo, SES, Postmark). Leave empty for
    /// Mailpit, which accepts mail without one.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>The provider's SMTP key. A secret: read it from the secret store, never commit it.</summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// The credentials to log in with, or null when no username is set so the client connects
    /// anonymously, as Mailpit expects.
    /// </summary>
    public NetworkCredential? CreateCredentials() =>
        Username.Length == 0 ? null : new NetworkCredential(Username, Password);
}
