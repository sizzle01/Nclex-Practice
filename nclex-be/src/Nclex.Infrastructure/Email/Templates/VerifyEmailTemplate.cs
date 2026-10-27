using Nclex.Application.Email;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>The "verify your email" email sent at sign-up and on resend.</summary>
internal sealed class VerifyEmailTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.VerifyEmail;

    public bool PayloadHoldsSecret => true;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<VerifyEmailPayload>(payload);
        return LinkEmailLayout.Render(new LinkEmail(
            Subject: "Verify your email address",
            DisplayName: values.DisplayName,
            Intro: "Thanks for signing up. Please confirm this is your email address so you can start practice tests.",
            ButtonLabel: "Verify my email",
            Url: links.VerifyEmail(values.Token),
            ExpiryNote: "This link works once and expires in 24 hours.",
            IgnoreNote: "If you didn't create an account, you can ignore this email."));
    }
}
