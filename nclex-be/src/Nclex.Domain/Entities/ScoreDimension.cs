namespace Nclex.Domain.Entities;

/// <summary>
/// A way of slicing an attempt's score for the results page (the <c>dimension</c> column of
/// <c>attempt_category_scores</c>, database.md section 2.7).
/// </summary>
public enum ScoreDimension
{
    /// <summary>The Client Needs category a question is tagged with, e.g. "SECE.MOC".</summary>
    ClientNeeds,

    /// <summary>The top-level Client Needs category, e.g. "SECE"; a top-level leaf counts as its own parent.</summary>
    ClientNeedsParent,
    BodySystem,
    ItemType,

    /// <summary>Only questions that target a clinical judgment step are counted.</summary>
    CjmmSkill,
    Difficulty,
}
