namespace Nclex.Infrastructure.Email.Templates;

/// <summary>Turns an outbox row's payload into an email. One class per <c>email_outbox.template</c> value.</summary>
public interface IEmailTemplate
{
    /// <summary>The template name stored in the outbox, from <see cref="Application.Email.EmailTemplateNames"/>.</summary>
    string Name { get; }

    /// <summary>True when the payload holds a secret, such as a link token, that must not be kept after sending.</summary>
    bool PayloadHoldsSecret { get; }

    EmailContent Render(string payload);
}

/// <summary>A rendered subject and body, in HTML and plain text.</summary>
public sealed record EmailContent(string Subject, string HtmlBody, string TextBody);
