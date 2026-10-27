using System.Text.Json;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Domain.Tests.Entities;

public sealed class QuestionTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 27, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid AuthorId = Guid.NewGuid();

    [Fact]
    public void Publishing_the_first_version_makes_it_current()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, AuthorId, Now);
        var version = QuestionVersion.CreateImported(question.Id, 1, Item(), AuthorId, Now);

        question.Publish(version, currentlyPublished: null, Now);

        question.CurrentVersionId.ShouldBe(version.Id);
        version.Status.ShouldBe(VersionStatus.Published);
        version.PublishedAt.ShouldBe(Now);
    }

    [Fact]
    public void Publishing_a_new_version_supersedes_the_old_one()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, AuthorId, Now);
        var first = QuestionVersion.CreateImported(question.Id, 1, Item(), AuthorId, Now);
        question.Publish(first, null, Now);
        var second = QuestionVersion.CreateImported(question.Id, 2, Item() with { Stem = "Edited" }, AuthorId, Now.AddDays(1));

        question.Publish(second, first, Now.AddDays(1));

        first.Status.ShouldBe(VersionStatus.Superseded);
        second.Status.ShouldBe(VersionStatus.Published);
        question.CurrentVersionId.ShouldBe(second.Id);
    }

    [Fact]
    public void Publishing_requires_the_actual_current_version()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, AuthorId, Now);
        var first = QuestionVersion.CreateImported(question.Id, 1, Item(), AuthorId, Now);
        question.Publish(first, null, Now);
        var second = QuestionVersion.CreateImported(question.Id, 2, Item(), AuthorId, Now);

        Should.Throw<InvalidOperationException>(() => question.Publish(second, currentlyPublished: null, Now));
    }

    [Fact]
    public void A_version_of_another_question_cannot_be_published()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, AuthorId, Now);
        var other = QuestionVersion.CreateImported(Guid.NewGuid(), 1, Item(), AuthorId, Now);

        Should.Throw<InvalidOperationException>(() => question.Publish(other, null, Now));
    }

    [Fact]
    public void A_published_version_cannot_be_published_again()
    {
        var question = Question.Create(Guid.NewGuid(), ExamTrack.RN, PlanTier.Free, AuthorId, Now);
        var version = QuestionVersion.CreateImported(question.Id, 1, Item(), AuthorId, Now);
        question.Publish(version, null, Now);

        Should.Throw<InvalidOperationException>(() => question.Publish(version, version, Now));
    }

    [Fact]
    public void Matches_ignores_json_key_order_and_spacing()
    {
        var version = QuestionVersion.CreateImported(Guid.NewGuid(), 1, Item(), AuthorId, Now);
        var reordered = Item() with { Content = Parse("""{"options":[{"text":"A","id":"a"},{"text":"B","id":"b"}]}""") };

        version.Matches(reordered).ShouldBeTrue();
        version.Matches(Item() with { AnswerKey = Parse("""{"correct":"b"}""") }).ShouldBeFalse();
        version.Matches(Item() with { Difficulty = DifficultyTier.Difficult }).ShouldBeFalse();
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
