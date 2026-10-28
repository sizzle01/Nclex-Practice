namespace Nclex.Domain.Flags;

/// <summary>
/// Where a report is in moderation (database enum <c>flag_status</c>, database.md section 2.1).
/// Members are in the enum's order, which is also the order Postgres sorts them in.
/// </summary>
public enum FlagStatus
{
    /// <summary>Just reported; nobody has looked at it yet.</summary>
    Open,

    /// <summary>A moderator has looked at it and confirmed it needs work.</summary>
    Triaged,

    /// <summary>Fixed or answered; the reporter was notified with the resolution note.</summary>
    Resolved,

    /// <summary>No change was needed. The note is kept for moderators only.</summary>
    Rejected,
}
