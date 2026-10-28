using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Tests.Entities;

public sealed class QuestionVersionWorkflowTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 28, 9, 0, 0, TimeSpan.Zero);
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly Guid Reviewer = Guid.NewGuid();

    [Fact]
    public void A_draft_goes_through_review_and_is_published()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, Author, Now);
        var draft = QuestionVersion.CreateDraft(question.Id, 1, Item(), Author, Now);

        draft.Status.ShouldBe(VersionStatus.Draft);
        draft.SubmitForReview();
        draft.Status.ShouldBe(VersionStatus.InReview);
        draft.Approve(Reviewer, "Looks right", Now.AddHours(1));
        draft.Status.ShouldBe(VersionStatus.Approved);
        draft.ReviewerId.ShouldBe(Reviewer);
        draft.ReviewedAt.ShouldBe(Now.AddHours(1));
        question.Publish(draft, null, Now.AddHours(2));

        draft.Status.ShouldBe(VersionStatus.Published);
        question.CurrentVersionId.ShouldBe(draft.Id);
    }

    [Fact]
    public void Editing_replaces_the_draft_content()
    {
        var draft = QuestionVersion.CreateDraft(Guid.NewGuid(), 1, Item(), Author, Now);

        draft.EditDraft(Item() with { Stem = "Edited stem", Difficulty = DifficultyTier.Difficult });

        draft.Stem.ShouldBe("Edited stem");
        draft.Difficulty.ShouldBe(DifficultyTier.Difficult);
    }

    [Fact]
    public void A_version_cannot_be_edited_once_submitted()
    {
        var draft = QuestionVersion.CreateDraft(Guid.NewGuid(), 1, Item(), Author, Now);
        draft.SubmitForReview();

        Should.Throw<InvalidOperationException>(() => draft.EditDraft(Item()));
    }

    [Fact]
    public void The_author_cannot_approve_their_own_version()
    {
        var draft = QuestionVersion.CreateDraft(Guid.NewGuid(), 1, Item(), Author, Now);
        draft.SubmitForReview();

        Should.Throw<InvalidOperationException>(() => draft.Approve(Author, null, Now));
        draft.Status.ShouldBe(VersionStatus.InReview);
    }

    [Fact]
    public void Rejecting_keeps_the_notes_for_the_author()
    {
        var draft = QuestionVersion.CreateDraft(Guid.NewGuid(), 1, Item(), Author, Now);
        draft.SubmitForReview();

        draft.Reject(Reviewer, "Option c's rationale is wrong", Now);

        draft.Status.ShouldBe(VersionStatus.Rejected);
        draft.ReviewNotes.ShouldBe("Option c's rationale is wrong");
    }

    [Fact]
    public void A_draft_cannot_skip_review()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, Author, Now);
        var draft = QuestionVersion.CreateDraft(question.Id, 1, Item(), Author, Now);

        Should.Throw<InvalidOperationException>(() => draft.Approve(Reviewer, null, Now));
        Should.Throw<InvalidOperationException>(() => question.Publish(draft, null, Now));
    }

    [Fact]
    public void A_clone_is_a_fresh_draft_with_the_same_content_and_a_new_author()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, Author, Now);
        var published = QuestionVersion.CreateDraft(question.Id, 1, Item(), Author, Now);
        published.SubmitForReview();
        published.Approve(Reviewer, "OK", Now);
        question.Publish(published, null, Now);
        var editor = Guid.NewGuid();

        var clone = published.CloneAsDraft(2, editor, Now.AddDays(1));

        clone.Id.ShouldNotBe(published.Id);
        clone.Version.ShouldBe(2);
        clone.Status.ShouldBe(VersionStatus.Draft);
        clone.AuthorId.ShouldBe(editor);
        clone.ReviewerId.ShouldBeNull();
        clone.ReviewNotes.ShouldBeNull();
        clone.Matches(published.ToAuthoredItem()).ShouldBeTrue();
    }

    [Fact]
    public void Retiring_twice_is_refused()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, Author, Now);

        question.Retire(Now);

        question.Status.ShouldBe(QuestionStatus.Retired);
        question.RetiredAt.ShouldBe(Now);
        Should.Throw<InvalidOperationException>(() => question.Retire(Now));
    }

    private static AuthoredItem Item() => new(
        ItemType.MultipleChoice,
        "Stem",
        Parse("""{ "options": [ { "id": "a", "text": "A" }, { "id": "b", "text": "B" } ] }"""),
        Parse("""{ "correct": "a" }"""),
        Parse("""{ "correct": "Why", "perOption": { "a": "A", "b": "B" } }"""),
        "Objective",
        Parse("[]"),
        ClientNeedsId: 2,
        BodySystemId: 1,
        CjmmSkill.TakeAction,
        DifficultyTier.Easy,
        TimeAllotmentSec: 120,
        MaxPoints: 1);

    private static JsonElement Parse(string json) => JsonSerializer.Deserialize<JsonElement>(json);
}
