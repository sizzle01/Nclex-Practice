namespace Nclex.Domain.Entities;

/// <summary>
/// Why an attempt is paused (<c>attempts.pause_reason</c>, api.md POST /pause). Both kinds follow the
/// same limit and overrun rules (architecture.md 5.1); they differ only in what the player shows.
/// </summary>
public enum PauseReason
{
    /// <summary>A break: the pause overlay hides the content until the candidate resumes.</summary>
    Break,

    /// <summary>"Save &amp; Exit": the candidate leaves the player and resumes from the dashboard.</summary>
    SaveExit,
}
