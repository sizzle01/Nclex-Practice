namespace Nclex.Domain.Entities;

/// <summary>When answers and rationales are shown (database enum <c>reveal_mode</c>).</summary>
public enum RevealMode
{
    /// <summary>After each item is submitted.</summary>
    Tutor,

    /// <summary>Only after the whole attempt is submitted.</summary>
    Exam,
}
