using Nclex.Application.Email;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>The "confirm your new email" email, sent to the address the user wants to move to.</summary>
internal sealed class ConfirmEmailChangeTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.ConfirmEmailChange;

    public bool PayloadHoldsSecret => true;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<ConfirmEmailChangePayload>(payload);
        return LinkEmailLayout.Render(new LinkEmail(
            Subject: "Confirm your new email address",
            DisplayName: values.DisplayName,
            Intro: $"Please confirm that you want to sign in to NCLEX Practice with {values.NewEmail} from now on.",
            ButtonLabel: "Confirm my new email",
            Url: links.ConfirmEmailChange(values.Token),
            ExpiryNote: "This link works once and expires in 24 hours.",
            IgnoreNote: "If you didn't ask for this, you can ignore this email; nothing will change."));
    }
}
