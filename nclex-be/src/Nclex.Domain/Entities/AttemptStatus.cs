namespace Nclex.Domain.Entities;

/// <summary>Where an attempt is in its lifecycle (database enum <c>attempt_status</c>, architecture.md 5.3).</summary>
public enum AttemptStatus
{
    /// <summary>Being taken; a draft that can be written to.</summary>
    InProgress,

    /// <summary>On a break or saved for later (F09); still a draft.</summary>
    Paused,

    /// <summary>Finished and graded (F10). Never written again.</summary>
    Submitted,

    /// <summary>A draft the candidate discarded. Never written again.</summary>
    Abandoned,
}
