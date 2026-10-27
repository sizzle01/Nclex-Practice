namespace Nclex.Application.Email;

/// <summary>Values of <c>email_outbox.template</c> (database.md section 2.8). Each has a payload record below.</summary>
public static class EmailTemplateNames
{
    public const string VerifyEmail = "verify_email";
    public const string ResetPassword = "reset_password";
    public const string ConfirmEmailChange = "confirm_email_change";

    // Security notices (no link token): sent after something changed on the account.
    public const string EmailChangeRequested = "email_change_requested";
    public const string EmailChanged = "email_changed";
    public const string PasswordChanged = "password_changed";
    public const string AccountLocked = "account_locked";
    public const string NewDeviceSignIn = "new_device_sign_in";
}

/// <summary>Payload of the "verify your email" email.</summary>
/// <param name="DisplayName">Used in the greeting, if the user gave one.</param>
/// <param name="Token">The raw link token. A secret: the worker clears it once the email is sent.</param>
public sealed record VerifyEmailPayload(string? DisplayName, string Token);

/// <summary>Payload of the "reset your password" email.</summary>
/// <param name="DisplayName">Used in the greeting, if the user gave one.</param>
/// <param name="Token">The raw link token. A secret: the worker clears it once the email is sent.</param>
public sealed record ResetPasswordPayload(string? DisplayName, string Token);

/// <summary>Payload of the "confirm your new email" email, sent to the new address.</summary>
/// <param name="DisplayName">Used in the greeting, if the user gave one.</param>
/// <param name="NewEmail">The address being confirmed.</param>
/// <param name="Token">The raw link token. A secret: the worker clears it once the email is sent.</param>
public sealed record ConfirmEmailChangePayload(string? DisplayName, string NewEmail, string Token);

/// <summary>Payload of the notice to the old address that someone asked to change it.</summary>
public sealed record EmailChangeRequestedPayload(string? DisplayName, string NewEmail);

/// <summary>Payload of the notice to the old address that the change went through.</summary>
public sealed record EmailChangedPayload(string? DisplayName, string NewEmail, DateTimeOffset ChangedAt);

/// <summary>Payload of the notice that the password was changed or reset.</summary>
public sealed record PasswordChangedPayload(string? DisplayName, DateTimeOffset ChangedAt);

/// <summary>Payload of the notice that the account was locked after failed attempts.</summary>
public sealed record AccountLockedPayload(string? DisplayName, DateTimeOffset LockedUntil);

/// <summary>Payload of the notice about a sign-in from a browser not seen before.</summary>
/// <param name="DisplayName">Used in the greeting, if the user gave one.</param>
/// <param name="SignedInAt">When the sign-in happened.</param>
/// <param name="Browser">The User-Agent string, if the browser sent one.</param>
/// <param name="IpAddress">The client's IP address, if known.</param>
public sealed record NewDeviceSignInPayload(string? DisplayName, DateTimeOffset SignedInAt, string? Browser, string? IpAddress);
