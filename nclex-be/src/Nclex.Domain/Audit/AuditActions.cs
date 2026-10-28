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

    // Admin console (F14). Every write in api.md section 8 records one of these.
    public const string QuestionCreated = "question.create";
    public const string QuestionRetired = "question.retire";
    public const string VersionCreated = "question_version.create";
    public const string VersionEdited = "question_version.edit";
    public const string VersionSubmitted = "question_version.submit_for_review";
    public const string VersionApproved = "question_version.approve";
    public const string VersionRejected = "question_version.reject";
    public const string VersionPublished = "question.publish";
    public const string MediaCreated = "media.create";
    public const string MediaUploaded = "media.upload";
    public const string MediaUpdated = "media.update";
    public const string TestSetCreated = "test_set.create";
    public const string TestSetUpdated = "test_set.update";
    public const string BankCreated = "question_bank.create";
    public const string BankUpdated = "question_bank.update";
    public const string UserUpdated = "user.update";

    /// <summary><c>entity_type</c> for events about a user account.</summary>
    public const string UserEntity = "user";

    public const string QuestionEntity = "question";
    public const string QuestionVersionEntity = "question_version";
    public const string MediaEntity = "media_asset";
    public const string TestSetEntity = "test_set";
    public const string BankEntity = "question_bank";
}
