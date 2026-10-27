namespace Nclex.Application.Users.ChangePassword;

/// <summary>Body of POST /me/password (api.md section 2).</summary>
/// <param name="CurrentPassword">Re-entered to confirm it is really the account owner.</param>
/// <param name="NewPassword">The new password; same rules as at sign-up.</param>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
