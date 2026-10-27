namespace Nclex.Application.Catalog.CustomSets;

/// <summary>Which of the user's questions the builder may use (api.md section 3, <c>pool</c>).</summary>
public enum PoolMode
{
    /// <summary>Every question that matches the filters.</summary>
    All,

    /// <summary>Only questions the user has never been served (user_question_history).</summary>
    Unused,

    /// <summary>Only questions whose latest answer was wrong.</summary>
    Incorrect,

    /// <summary>
    /// Questions the user marked for review in an attempt. Attempts arrive with F08, so this pool
    /// is empty until then.
    /// </summary>
    Flagged,
}
