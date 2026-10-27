namespace Nclex.Application.Users.ChangeEmail;

/// <summary>Body of POST /me/email (api.md section 2).</summary>
/// <param name="CurrentPassword">Re-entered to confirm it is really the account owner.</param>
/// <param name="NewEmail">The address to move the account to, once confirmed from that inbox.</param>
public sealed record ChangeEmailRequest(string CurrentPassword, string NewEmail);
