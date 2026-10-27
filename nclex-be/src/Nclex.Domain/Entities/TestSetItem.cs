namespace Nclex.Domain.Entities;

/// <summary>
/// One question in a test set, at a fixed position. Maps to <c>test_set_items</c> (database.md
/// section 2.5). It points at the question, not a version: the current published version is
/// chosen when an attempt starts.
/// </summary>
public sealed class TestSetItem
{
    // For EF Core, which sets properties directly when loading rows.
    private TestSetItem()
    {
    }

    public Guid TestSetId { get; private set; }

    /// <summary>1, 2, 3... in the order the items are served.</summary>
    public int Position { get; private set; }

    public Guid QuestionId { get; private set; }

    /// <summary>Builds a set's items from question ids, numbering them from 1 in the order given.</summary>
    public static IReadOnlyList<TestSetItem> ForQuestions(Guid testSetId, IEnumerable<Guid> questionIds) =>
        [.. questionIds.Select((questionId, index) => new TestSetItem
        {
            TestSetId = testSetId,
            Position = index + 1,
            QuestionId = questionId,
        })];
}
