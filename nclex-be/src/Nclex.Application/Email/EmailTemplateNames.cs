namespace Nclex.Application.Email;

/// <summary>Values of <c>email_outbox.template</c> (database.md section 2.8). Each has a payload record below.</summary>
public static class EmailTemplateNames
{
    public const string VerifyEmail = "verify_email";
    public const string ResetPassword = "reset_password";
}

/// <summary>Payload of the "verify your email" email.</summary>
/// <param name="DisplayName">Used in the greeting, if the user gave one.</param>
/// <param name="Token">The raw link token. A secret: the worker clears it once the email is sent.</param>
public sealed record VerifyEmailPayload(string? DisplayName, string Token);

/// <summary>Payload of the "reset your password" email.</summary>
/// <param name="DisplayName">Used in the greeting, if the user gave one.</param>
/// <param name="Token">The raw link token. A secret: the worker clears it once the email is sent.</param>
public sealed record ResetPasswordPayload(string? DisplayName, string Token);
