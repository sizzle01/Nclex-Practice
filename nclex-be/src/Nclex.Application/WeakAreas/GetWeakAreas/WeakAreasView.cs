using Nclex.Domain.Entities;

namespace Nclex.Application.WeakAreas.GetWeakAreas;

/// <summary>Body of GET /me/weak-areas: what the "Practice your weak areas" card shows before the click.</summary>
/// <param name="Areas">The weak Client Needs leaves, worst first; empty when nothing qualifies.</param>
/// <param name="AttemptsConsidered">How many recent submitted attempts were looked at (at most 10).</param>
/// <param name="QuestionsAvailable">Questions in the weak areas the test could use.</param>
/// <param name="QuestionCount">How many the test would have: 25, fewer when the areas run short, 0 when no test is possible.</param>
public sealed record WeakAreasView(
    ExamTrack Track,
    IReadOnlyList<WeakAreaView> Areas,
    int AttemptsConsidered,
    int QuestionsAvailable,
    int QuestionCount);
