namespace Nclex.Domain.Entities;

/// <summary>
/// Tags a question with a subtopic. Maps to <c>question_subtopics</c> (database.md section 2.4). It
/// points at the question, not a version: published versions can't change, and a tag is study
/// metadata, so correcting one needs no new version or review and also improves past results.
/// </summary>
public sealed class QuestionSubtopic
{
    // For EF Core, which sets properties directly when loading rows.
    private QuestionSubtopic()
    {
    }

    public Guid QuestionId { get; private set; }

    public Guid SubtopicId { get; private set; }

    public static QuestionSubtopic Create(Guid questionId, Guid subtopicId) => new() { QuestionId = questionId, SubtopicId = subtopicId };
}
