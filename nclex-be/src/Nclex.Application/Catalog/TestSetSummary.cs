using System.Linq.Expressions;
using Nclex.Domain.Entities;

namespace Nclex.Application.Catalog;

/// <summary>A set as shown on a card in the catalog (api.md section 3, GET /test-sets).</summary>
/// <param name="QuestionCount">How many items the set has.</param>
/// <param name="PassThreshold">Share of points for a "likely pass" estimate, e.g. 0.65; null for none.</param>
/// <param name="IsSample">Can be started before verifying an email.</param>
/// <param name="Locked">True when the set is premium and the user's plan is free.</param>
/// <param name="MyLastAttempt">The user's latest submitted attempt of the set; null when they have never finished it.</param>
public sealed record TestSetSummary(
    Guid Id,
    string Title,
    TestMode Mode,
    RevealMode RevealMode,
    int QuestionCount,
    bool Timed,
    int? TimeBudgetSec,
    int? PerQuestionSec,
    decimal? PassThreshold,
    bool IsSample,
    PlanTier AccessTier,
    bool Locked,
    LastAttemptSummary? MyLastAttempt);

/// <summary>The user's latest submitted attempt of a set.</summary>
/// <param name="ScorePct">Share of points earned, from 0 to 1.</param>
public sealed record LastAttemptSummary(Guid Id, decimal ScorePct, DateTimeOffset SubmittedAt);

/// <summary>A set with everything the detail panel shows (api.md section 3, GET /test-sets/{id}).</summary>
/// <param name="AllowBackNav">Whether candidates may go back to earlier items.</param>
/// <param name="PausePolicy">Pause limits; null values mean no limit.</param>
/// <param name="Locked">True when the set is premium and the user's plan is free.</param>
/// <param name="MyLastAttempt">The user's latest submitted attempt of the set; null when they have never finished it.</param>
public sealed record TestSetDetail(
    Guid Id,
    ExamTrack Track,
    TestSetKind Kind,
    string Title,
    string? Description,
    TestMode Mode,
    RevealMode RevealMode,
    int QuestionCount,
    bool Timed,
    int? TimeBudgetSec,
    int? PerQuestionSec,
    decimal? PassThreshold,
    PausePolicy PausePolicy,
    bool AllowBackNav,
    bool IsSample,
    PlanTier AccessTier,
    bool Locked,
    LastAttemptSummary? MyLastAttempt)
{
    /// <summary>The detail of <paramref name="set"/> as <paramref name="viewer"/> sees it.</summary>
    public static TestSetDetail From(TestSet set, CatalogViewer viewer, LastAttemptSummary? myLastAttempt = null) => new(
        set.Id,
        set.Track,
        set.Kind,
        set.Title,
        set.Description,
        set.Mode,
        set.RevealMode,
        set.QuestionCount,
        set.Timed,
        set.TimeBudgetSec,
        set.PerQuestionSec,
        set.PassThreshold,
        set.PausePolicy,
        set.AllowBackNav,
        set.IsSample,
        set.AccessTier,
        viewer.IsLocked(set.AccessTier),
        myLastAttempt);
}

/// <summary>Which sets a user may see at all; locking is separate.</summary>
internal static class TestSetVisibility
{
    /// <summary>Published curated sets for everyone, plus the user's own custom sets (F07).</summary>
    public static Expression<Func<TestSet, bool>> VisibleTo(Guid userId) =>
        s => (s.IsPublished && s.Kind != TestSetKind.Custom) || (s.Kind == TestSetKind.Custom && s.OwnerUserId == userId);
}
