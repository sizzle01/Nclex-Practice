namespace Nclex.Application.Abstractions;

/// <summary>Hands a rendered email to the email provider (SMTP locally, Mailpit in development).</summary>
public interface IEmailSender
{
    /// <summary>Sends the email and returns the provider's id for it. Throws if the provider rejects it.</summary>
    Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

/// <summary>A rendered email with an HTML body and a plain-text alternative.</summary>
public sealed record EmailMessage(string To, string Subject, string HtmlBody, string TextBody);
