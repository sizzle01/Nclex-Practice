using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using Microsoft.Extensions.Options;
using Nclex.Application.Abstractions;

namespace Nclex.Infrastructure.Email;

/// <summary>
/// Sends email over SMTP with the built-in <see cref="SmtpClient"/>: anonymously to Mailpit
/// locally, or with a username and SMTP key to a provider such as Brevo. A provider's HTTP API
/// can replace this class behind <see cref="IEmailSender"/>.
/// </summary>
internal sealed class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    public async Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var from = new MailAddress(settings.From);

        // SmtpClient does not report an id for the message, so we set our own Message-ID header
        // and store that as email_outbox.provider_id.
        var messageId = $"<{Guid.NewGuid():N}@{from.Host}>";

        using var mail = new MailMessage
        {
            From = from,
            Subject = message.Subject,
            SubjectEncoding = Encoding.UTF8,
        };
        mail.To.Add(message.To);
        mail.Headers.Add("Message-ID", messageId);
        // Plain text first: mail clients show the last alternative they can display.
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.TextBody, Encoding.UTF8, MediaTypeNames.Text.Plain));
        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message.HtmlBody, Encoding.UTF8, MediaTypeNames.Text.Html));

        using var client = new SmtpClient(settings.Smtp.Host, settings.Smtp.Port)
        {
            EnableSsl = settings.Smtp.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };
        // Set only when there is a login: assigning null keeps SmtpClient anonymous for Mailpit.
        var credentials = settings.Smtp.CreateCredentials();
        if (credentials is not null)
        {
            client.Credentials = credentials;
        }
        await client.SendMailAsync(mail, cancellationToken);
        return messageId;
    }
}
