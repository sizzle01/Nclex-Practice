namespace Nclex.Domain.Entities;

/// <summary>How far a candidate has got with one item of an attempt (database enum <c>item_state</c>).</summary>
public enum ItemState
{
    /// <summary>Never shown.</summary>
    Unseen,

    /// <summary>Shown, with no answer saved.</summary>
    Seen,

    /// <summary>Has a saved answer that can still change.</summary>
    Answered,

    /// <summary>Final: submitted in tutor mode, its item timer expired, or left when back-navigation is off.</summary>
    Locked,
}
