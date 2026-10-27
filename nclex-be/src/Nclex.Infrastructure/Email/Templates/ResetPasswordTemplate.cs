using Nclex.Application.Email;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>The "reset your password" email sent from the forgot-password page.</summary>
internal sealed class ResetPasswordTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.ResetPassword;

    public bool PayloadHoldsSecret => true;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<ResetPasswordPayload>(payload);
        return LinkEmailLayout.Render(new LinkEmail(
            Subject: "Reset your password",
            DisplayName: values.DisplayName,
            Intro: "We received a request to reset the password for your NCLEX Practice account.",
            ButtonLabel: "Choose a new password",
            Url: links.ResetPassword(values.Token),
            ExpiryNote: "This link works once and expires in 30 minutes.",
            IgnoreNote: "If you didn't ask for this, you can ignore this email; your password won't change."));
    }
}
