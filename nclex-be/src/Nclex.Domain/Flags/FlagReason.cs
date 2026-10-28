namespace Nclex.Domain.Flags;

/// <summary>
/// Why a candidate reported a question (database enum <c>flag_reason</c>, database.md section 2.1).
/// Members are in the enum's order, which is also the order Postgres sorts them in.
/// </summary>
public enum FlagReason
{
    /// <summary>The rationale does not explain the answer well enough.</summary>
    RationaleUnclear,

    /// <summary>The candidate thinks the keyed answer is wrong.</summary>
    IncorrectAnswerKey,

    /// <summary>A spelling or wording mistake.</summary>
    Typo,

    /// <summary>The question follows guidance that has since changed.</summary>
    OutdatedGuideline,

    /// <summary>An image, audio or video exhibit does not load or is hard to use.</summary>
    MediaProblem,

    /// <summary>Anything else; the comment says what.</summary>
    Other,
}
