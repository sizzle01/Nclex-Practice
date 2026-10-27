using Nclex.Application.Email;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>Notice to the current address that someone asked to move the account to another one.</summary>
internal sealed class EmailChangeRequestedTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.EmailChangeRequested;

    public bool PayloadHoldsSecret => false;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<EmailChangeRequestedPayload>(payload);
        return NoticeEmailLayout.Render(new NoticeEmail(
            Subject: "A change of email address was requested",
            DisplayName: values.DisplayName,
            Paragraphs:
            [
                $"Someone signed in to your account asked to change its email address to {values.NewEmail}.",
                "Nothing changes until the link we sent to that address is followed. You'll keep signing in with this address until then.",
            ],
            IfNotYou: "If this wasn't you, someone may know your password. Reset it now to cancel the change.",
            ActionLabel: "Reset my password",
            ActionUrl: links.ForgotPassword()));
    }
}
