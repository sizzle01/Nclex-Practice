namespace Nclex.Application.Attempts.Recommendations;

/// <summary>Body of GET /attempts/{id}/recommendations: subtopic names and counts only, never keys or rationales.</summary>
/// <param name="Subtopics">What to study next, most important first; empty when nothing stands out.</param>
public sealed record AttemptRecommendationsView(Guid AttemptId, IReadOnlyList<RecommendedSubtopic> Subtopics);

/// <summary>A subtopic to study.</summary>
/// <param name="Missed">Answered questions on this subtopic without full marks.</param>
/// <param name="Answered">Questions answered on this subtopic.</param>
public sealed record RecommendedSubtopic(Guid Id, string Name, int Missed, int Answered);
