namespace Nclex.Domain.Entities;

/// <summary>Why an attempt was submitted (database enum <c>submit_reason</c>).</summary>
public enum SubmitReason
{
    /// <summary>The candidate pressed Submit.</summary>
    User,

    /// <summary>The exam timer ran out (F09).</summary>
    TimeExpired,

    /// <summary>The adaptive exam ended on its own: a clear result, the item maximum, or no questions left (F19).</summary>
    CatStop,
}
