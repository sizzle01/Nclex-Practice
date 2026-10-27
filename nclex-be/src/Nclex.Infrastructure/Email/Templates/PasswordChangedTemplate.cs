using Nclex.Application.Email;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>Notice that the password was changed in settings or reset from an emailed link.</summary>
internal sealed class PasswordChangedTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.PasswordChanged;

    public bool PayloadHoldsSecret => false;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<PasswordChangedPayload>(payload);
        return NoticeEmailLayout.Render(new NoticeEmail(
            Subject: "Your password was changed",
            DisplayName: values.DisplayName,
            Paragraphs: [$"The password for your NCLEX Practice account was changed on {NoticeEmailLayout.FormatTime(values.ChangedAt)}."],
            IfNotYou: "If this wasn't you, reset your password now to take your account back.",
            ActionLabel: "Reset my password",
            ActionUrl: links.ForgotPassword()));
    }
}
