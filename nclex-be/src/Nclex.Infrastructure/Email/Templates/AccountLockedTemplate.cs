using Nclex.Application.Email;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>Notice that the account was locked after too many wrong passwords.</summary>
internal sealed class AccountLockedTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.AccountLocked;

    public bool PayloadHoldsSecret => false;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<AccountLockedPayload>(payload);
        return NoticeEmailLayout.Render(new NoticeEmail(
            Subject: "Your account was temporarily locked",
            DisplayName: values.DisplayName,
            Paragraphs:
            [
                "We locked your NCLEX Practice account because the wrong password was entered too many times.",
                $"You can sign in again after {NoticeEmailLayout.FormatTime(values.LockedUntil)}.",
            ],
            IfNotYou: "If this wasn't you, someone may be trying to guess your password. Choosing a new, unique password keeps them out.",
            ActionLabel: "Reset my password",
            ActionUrl: links.ForgotPassword()));
    }
}
