namespace Nclex.Domain.Entities;

/// <summary>Whether a question can be put into new test sets (database enum <c>question_status</c>).</summary>
public enum QuestionStatus
{
    Active,

    /// <summary>Removed from future sets; past attempts that used it are unaffected.</summary>
    Retired,
}
