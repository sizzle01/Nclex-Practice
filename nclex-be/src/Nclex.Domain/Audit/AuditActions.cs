namespace Nclex.Domain.Audit;

/// <summary>Values of <c>audit_events.action</c> (database.md section 2.9 gives the dotted style).</summary>
public static class AuditActions
{
    public const string LoginSucceeded = "auth.login_succeeded";
    public const string LoginFailed = "auth.login_failed";
    public const string AccountLocked = "auth.account_locked";
    public const string PasswordChanged = "auth.password_changed";
    public const string PasswordReset = "auth.password_reset";
    public const string EmailChangeRequested = "auth.email_change_requested";
    public const string EmailChanged = "auth.email_changed";
    public const string AccountDeleted = "account.deleted";

    /// <summary><c>entity_type</c> for events about a user account.</summary>
    public const string UserEntity = "user";
}
