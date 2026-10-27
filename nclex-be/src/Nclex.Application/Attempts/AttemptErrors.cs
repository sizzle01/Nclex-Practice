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

    /// <summary>Item content is hidden while paused (PRD 5.4); pausing itself arrives in F09.</summary>
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
}
