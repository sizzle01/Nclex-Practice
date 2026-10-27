namespace Nclex.Domain.Entities;

/// <summary>What an emailed link is for. Maps to the <c>email_token_purpose</c> enum (database.md section 2.1).</summary>
public enum EmailTokenPurpose
{
    /// <summary>Confirms that the user owns their sign-up email address.</summary>
    VerifyEmail,

    /// <summary>Lets the user choose a new password after "forgot password".</summary>
    ResetPassword,

    /// <summary>Confirms a new email address (built in F03).</summary>
    ChangeEmail,
}
