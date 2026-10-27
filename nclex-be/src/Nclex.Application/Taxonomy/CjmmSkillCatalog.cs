using Nclex.Domain.Entities;

namespace Nclex.Application.Taxonomy;

/// <summary>Display text for a Clinical Judgment Measurement Model step.</summary>
/// <param name="Key">The enum value, sent as snake_case, e.g. "recognize_cues".</param>
/// <param name="Label">Short name, e.g. "Recognize cues".</param>
/// <param name="Prompt">The question a nurse asks at this step, e.g. "What matters most?".</param>
public sealed record CjmmSkillInfo(CjmmSkill Key, string Label, string Prompt);

/// <summary>
/// Labels and prompts for the six CJMM steps, in model order. The database stores only the enum
/// (database.md 2.1), so the text lives here.
/// </summary>
public static class CjmmSkillCatalog
{
    public static IReadOnlyList<CjmmSkillInfo> All { get; } =
    [
        new(CjmmSkill.RecognizeCues, "Recognize cues", "What matters most?"),
        new(CjmmSkill.AnalyzeCues, "Analyze cues", "What could it mean?"),
        new(CjmmSkill.PrioritizeHypotheses, "Prioritize hypotheses", "Where do I start?"),
        new(CjmmSkill.GenerateSolutions, "Generate solutions", "What can I do?"),
        new(CjmmSkill.TakeAction, "Take action", "What will I do?"),
        new(CjmmSkill.EvaluateOutcomes, "Evaluate outcomes", "Did it help?"),
    ];
}
