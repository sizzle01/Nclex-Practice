namespace Nclex.Application.Auth.ResetPassword;

/// <summary>Body of POST /auth/password/reset (api.md section 2).</summary>
/// <param name="Token">The token from the emailed reset link.</param>
/// <param name="NewPassword">The new password; same rules as at sign-up.</param>
/// <param name="SignOutEverywhere">Revoke every existing session. Defaults to true when omitted.</param>
public sealed record ResetPasswordRequest(string Token, string NewPassword, bool SignOutEverywhere = true);
