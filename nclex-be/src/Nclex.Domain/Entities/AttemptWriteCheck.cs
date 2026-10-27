namespace Nclex.Domain.Entities;

/// <summary>Whether a client may change an attempt right now, and if not, why (api.md section 1 codes).</summary>
public enum AttemptWriteCheck
{
    Allowed,

    /// <summary>The attempt is submitted or abandoned: ATTEMPT_NOT_ACTIVE.</summary>
    NotActive,

    /// <summary>The attempt is paused: ATTEMPT_PAUSED.</summary>
    Paused,

    /// <summary>Another device holds the lease: ATTEMPT_OPENED_ELSEWHERE.</summary>
    OpenedElsewhere,
}

/// <summary>What happened to an autosave.</summary>
public enum SaveOutcome
{
    Saved,

    /// <summary>
    /// The write's <c>clientSeq</c> is not newer than the stored one, so it was ignored. Not an error:
    /// retries and out-of-order deliveries are expected (architecture.md 5.2).
    /// </summary>
    Stale,

    /// <summary>The item is locked and is never written again (database.md 2.6 invariants).</summary>
    ItemLocked,
}

/// <summary>What happened to a request to move to another item.</summary>
public enum NavigationOutcome
{
    Moved,

    /// <summary>The set does not allow going back: BACK_NAVIGATION_DISABLED.</summary>
    BackNavigationDisabled,
}
