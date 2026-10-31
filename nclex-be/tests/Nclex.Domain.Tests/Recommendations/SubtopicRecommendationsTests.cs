using Nclex.Domain.Recommendations;

namespace Nclex.Domain.Tests.Recommendations;

public sealed class SubtopicRecommendationsTests
{
    private static readonly Guid Insulin = Guid.NewGuid();
    private static readonly Guid Anticoagulants = Guid.NewGuid();
    private static readonly Guid Interactions = Guid.NewGuid();

    private static IEnumerable<TaggedAnswer> Answers(Guid id, string name, int correct, int missed) =>
        Enumerable.Repeat(new TaggedAnswer(id, name, true), correct)
            .Concat(Enumerable.Repeat(new TaggedAnswer(id, name, false), missed));

    [Fact]
    public void Most_missed_comes_first_and_counts_are_per_subtopic()
    {
        var gaps = SubtopicRecommendations.From([
            .. Answers(Insulin, "Insulin administration", correct: 1, missed: 1),
            .. Answers(Anticoagulants, "Anticoagulants", correct: 3, missed: 2)]);

        gaps.Select(g => (g.Name, g.Missed, g.Answered)).ShouldBe([("Anticoagulants", 2, 5), ("Insulin administration", 1, 2)]);
    }

    [Fact]
    public void With_equal_misses_the_lowest_share_correct_comes_first()
    {
        var gaps = SubtopicRecommendations.From([
            .. Answers(Insulin, "Insulin administration", correct: 1, missed: 2),
            .. Answers(Anticoagulants, "Anticoagulants", correct: 0, missed: 2)]);

        gaps.Select(g => g.Name).ShouldBe(["Anticoagulants", "Insulin administration"]);
    }

    [Fact]
    public void A_full_tie_is_broken_by_name_so_the_order_never_changes()
    {
        var gaps = SubtopicRecommendations.From([
            .. Answers(Insulin, "Insulin administration", correct: 0, missed: 1),
            .. Answers(Interactions, "Drug interactions", correct: 0, missed: 1)]);

        gaps.Select(g => g.Name).ShouldBe(["Drug interactions", "Insulin administration"]);
    }

    [Fact]
    public void A_subtopic_with_no_misses_is_not_suggested()
    {
        SubtopicRecommendations.From(Answers(Insulin, "Insulin administration", correct: 4, missed: 0)).ShouldBeEmpty();
    }

    [Fact]
    public void No_answers_means_no_suggestions()
    {
        SubtopicRecommendations.From([]).ShouldBeEmpty();
    }

    [Fact]
    public void One_question_with_two_tags_counts_toward_both()
    {
        var gaps = SubtopicRecommendations.From([new TaggedAnswer(Insulin, "Insulin administration", false), new TaggedAnswer(Interactions, "Drug interactions", false)]);

        gaps.Select(g => g.SubtopicId).ShouldBe([Interactions, Insulin]);
    }

    [Fact]
    public void Only_the_top_five_are_returned()
    {
        var answers = Enumerable.Range(0, 7).SelectMany(i => Answers(Guid.NewGuid(), $"Topic {i}", correct: 0, missed: i + 1));

        SubtopicRecommendations.From(answers).Select(g => g.Name).ShouldBe(["Topic 6", "Topic 5", "Topic 4", "Topic 3", "Topic 2"]);
    }
}
