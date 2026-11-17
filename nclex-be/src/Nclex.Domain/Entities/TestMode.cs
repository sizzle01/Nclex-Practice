namespace Nclex.Domain.Entities;

/// <summary>How a test set is meant to be taken (database enum <c>test_mode</c>).</summary>
public enum TestMode
{
    /// <summary>Learning at your own pace.</summary>
    Practice,

    /// <summary>Exam conditions, with a pass/fail estimate.</summary>
    Challenge,

    /// <summary>Computerised adaptive testing: items are picked one at a time from the answers so far (architecture.md 5.8).</summary>
    Cat,
}
