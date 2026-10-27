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
}
