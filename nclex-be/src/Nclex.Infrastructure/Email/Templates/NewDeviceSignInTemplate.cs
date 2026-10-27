using Nclex.Application.Email;

namespace Nclex.Infrastructure.Email.Templates;

/// <summary>Notice about a sign-in from a browser the account has not used recently.</summary>
internal sealed class NewDeviceSignInTemplate(EmailLinks links) : IEmailTemplate
{
    public string Name => EmailTemplateNames.NewDeviceSignIn;

    public bool PayloadHoldsSecret => false;

    public EmailContent Render(string payload)
    {
        var values = EmailPayloadJson.Read<NewDeviceSignInPayload>(payload);
        return NoticeEmailLayout.Render(new NoticeEmail(
            Subject: "New sign-in to your account",
            DisplayName: values.DisplayName,
            Paragraphs:
            [
                $"Your NCLEX Practice account was signed in to from a new browser on {NoticeEmailLayout.FormatTime(values.SignedInAt)}.",
                $"Browser: {values.Browser ?? "unknown"}. IP address: {values.IpAddress ?? "unknown"}.",
            ],
            IfNotYou: "If this wasn't you, reset your password now; that also lets you sign out every other device.",
            ActionLabel: "Reset my password",
            ActionUrl: links.ForgotPassword()));
    }
}
