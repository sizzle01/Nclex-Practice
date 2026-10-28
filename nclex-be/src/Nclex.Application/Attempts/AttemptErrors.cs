using Nclex.Application.Common;

namespace Nclex.Application.Attempts;

/// <summary>Errors returned by the attempt endpoints (api.md section 1).</summary>
public static class AttemptErrors
{
    /// <summary>The attempt does not exist or belongs to someone else; both look the same to the caller.</summary>
    public static readonly AppError AttemptNotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That attempt was not found.");

    /// <summary>There is no item at that position.</summary>
    public static readonly AppError ItemNotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "That item was not found.");

    /// <summary>Every question in the set is retired or unpublished, so there is nothing to serve.</summary>
    public static readonly AppError NoQuestionsAvailable = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "This test set has no questions available right now.");

    public static readonly AppError NotActive = new(
        "ATTEMPT_NOT_ACTIVE",
        AppErrorKind.Conflict,
        "This attempt has already been submitted or discarded.");

    /// <summary>
    /// The exam timer ran out and the attempt has been finalized (architecture.md 5.1). The player
    /// routes to results on this code.
    /// </summary>
    public static readonly AppError TimeExpired = new(
        "ATTEMPT_TIME_EXPIRED",
        AppErrorKind.Conflict,
        "The time limit for this attempt has passed.");

    public static readonly AppError PauseLimitReached = new(
        "PAUSE_LIMIT_REACHED",
        AppErrorKind.Conflict,
        "You have used all the pauses this test allows.");

    /// <summary>Item content is hidden while paused (PRD 5.4).</summary>
    public static readonly AppError Paused = new(
        "ATTEMPT_PAUSED",
        AppErrorKind.Locked,
        "This attempt is paused.");

    /// <summary>Another device holds the lease; the player offers "Continue here" (architecture.md 5.2).</summary>
    public static readonly AppError OpenedElsewhere = new(
        "ATTEMPT_OPENED_ELSEWHERE",
        AppErrorKind.Conflict,
        "This attempt is open on another device.");

    public static readonly AppError ItemLocked = new(
        "ITEM_LOCKED",
        AppErrorKind.Conflict,
        "This item is locked and can no longer be changed.");

    public static readonly AppError BackNavigationDisabled = new(
        "BACK_NAVIGATION_DISABLED",
        AppErrorKind.Conflict,
        "This test does not allow going back to earlier items.");

    /// <summary>
    /// Items are submitted one by one only in tutor mode; exam mode reveals everything after the
    /// final Submit (api.md, POST /items/{position}/submit returns 409 in exam mode).
    /// </summary>
    public static readonly AppError TutorModeOnly = new(
        "TUTOR_MODE_ONLY",
        AppErrorKind.Conflict,
        "Answers in this test are shown after you submit the whole test.");

    /// <summary>The attempt is still a draft, so it has no result yet.</summary>
    public static readonly AppError NotSubmitted = new(
        "ATTEMPT_NOT_ACTIVE",
        AppErrorKind.Conflict,
        "This attempt has not been submitted yet.");

    /// <summary>
    /// A submitted attempt with no stored result: one finalized before grading existed (F10).
    /// </summary>
    public static readonly AppError ResultNotFound = new(
        "NOT_FOUND",
        AppErrorKind.NotFound,
        "This attempt has no result.");
}
