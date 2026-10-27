using Microsoft.Extensions.Options;

namespace Nclex.Infrastructure.Email;

/// <summary>Builds the links in emails, pointing at frontend routes (interfaces.md section 5.2).</summary>
public sealed class EmailLinks(IOptions<EmailOptions> options)
{
    public string VerifyEmail(string token) => Build("/verify-email", token);

    public string ResetPassword(string token) => Build("/reset-password", token);

    private string Build(string path, string token) =>
        $"{options.Value.FrontendBaseUrl.TrimEnd('/')}{path}?token={Uri.EscapeDataString(token)}";
}
