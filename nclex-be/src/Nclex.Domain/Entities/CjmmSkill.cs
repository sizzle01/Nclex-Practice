namespace Nclex.Domain.Entities;

/// <summary>
/// The step of the NCSBN Clinical Judgment Measurement Model a question tests
/// (database enum <c>cjmm_skill</c>). Optional on a question: older-style items may not target one.
/// </summary>
public enum CjmmSkill
{
    RecognizeCues,
    AnalyzeCues,
    PrioritizeHypotheses,
    GenerateSolutions,
    TakeAction,
    EvaluateOutcomes,
}
