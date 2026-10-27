namespace Nclex.Application.Users.DeleteAccount;

/// <summary>Body of DELETE /me (api.md section 2).</summary>
/// <param name="CurrentPassword">Re-entered to confirm it is really the account owner.</param>
public sealed record DeleteAccountRequest(string CurrentPassword);
