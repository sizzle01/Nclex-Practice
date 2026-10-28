using Microsoft.Extensions.Options;

namespace Nclex.Infrastructure.Email;

/// <summary>Builds the links in emails, pointing at frontend routes (interfaces.md section 5.2).</summary>
public sealed class EmailLinks(IOptions<EmailOptions> options)
{
    public string VerifyEmail(string token) => Build("/verify-email", token);

    public string ResetPassword(string token) => Build("/reset-password", token);

    public string ConfirmEmailChange(string token) => Build("/confirm-email-change", token);

    /// <summary>The forgot-password page, offered in notices as "secure your account".</summary>
    public string ForgotPassword() => Page("/forgot-password");

    /// <summary>The account settings page (interfaces.md section 5.2).</summary>
    public string AccountSettings() => Page("/settings/account");

    /// <summary>The notification settings page, linked from every notification email.</summary>
    public string NotificationSettings() => Page("/settings/notifications");

    /// <summary>A page of the app; <paramref name="path"/> is a deep link such as "/attempts/{id}/results#breakdown".</summary>
    public string App(string path) => Page(path);

    private string Page(string path) => $"{options.Value.FrontendBaseUrl.TrimEnd('/')}{path}";

    private string Build(string path, string token) => $"{Page(path)}?token={Uri.EscapeDataString(token)}";
}
