using Nclex.Application.Email;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>Notice to the old address that the account now uses a new one.</summary>
internal sealed class EmailChangedTemplate : IEmailTemplate
{
    public string Name => EmailTemplateNames.EmailChanged;

    public bool PayloadHoldsSecret => false;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<EmailChangedPayload>(payload);
        return NoticeEmailLayout.Render(new NoticeEmail(
            Subject: "Your email address was changed",
            DisplayName: values.DisplayName,
            Paragraphs:
            [
                $"On {NoticeEmailLayout.FormatTime(values.ChangedAt)}, the email address for your NCLEX Practice account was changed to {values.NewEmail}.",
                "This address will no longer receive emails about the account.",
            ],
            // No reset link: it would go to the new address, which is the one in question.
            IfNotYou: "If this wasn't you, contact NCLEX Practice support straight away so we can help you recover your account."));
    }
}
