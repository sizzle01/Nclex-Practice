using Nclex.Domain.Builder;
using Nclex.Domain.Cat;

namespace Nclex.Domain.Tests.Cat;

public sealed class CatStateTests
{
    [Fact]
    public void A_new_state_holds_the_prior_and_the_first_question()
    {
        var first = Item();

        var state = CatState.Begin(CatConfig.NclexRn, first);

        state.Estimate.ShouldBe(AbilityEstimate.Prior);
        state.Items.ShouldBe([first]);
        state.AnsweredCount.ShouldBe(0);
    }

    [Fact]
    public void Answering_scores_the_current_question_and_updates_the_estimate()
    {
        var state = CatState.Begin(CatConfig.NclexRn, Item()).RecordAnswer(1);

        state.AnsweredCount.ShouldBe(1);
        state.Items[0].Score.ShouldBe(1);
        state.Theta.ShouldBeGreaterThan(0);
        state.StandardError.ShouldBeLessThan(1);
    }

    [Fact]
    public void A_question_cannot_be_answered_twice_or_given_twice()
    {
        var first = Item();
        var answered = CatState.Begin(CatConfig.NclexRn, first).RecordAnswer(0);

        Should.Throw<InvalidOperationException>(() => answered.RecordAnswer(1));
        Should.Throw<InvalidOperationException>(() => answered.Give(first));
    }

    [Fact]
    public void The_next_question_waits_for_an_answer_to_the_current_one()
    {
        var state = CatState.Begin(CatConfig.NclexRn, Item());

        Should.Throw<InvalidOperationException>(() => state.Give(Item()));
    }

    [Fact]
    public void Content_counts_include_the_current_question()
    {
        var state = CatState.Begin(CatConfig.NclexRn, Item(leaf: 2)).RecordAnswer(1).Give(Item(leaf: 3));

        state.ContentCounts.ShouldBe(new Dictionary<short, int> { [2] = 1, [3] = 1 }, ignoreOrder: true);
    }

    [Fact]
    public void The_state_survives_a_round_trip_through_json()
    {
        var state = CatState.Begin(CatConfig.NclexRn, Item()).RecordAnswer(0.5).Give(Item()).Stop(CatStopReason.ClearResult);

        var copy = CatState.FromJson(state.ToJson());

        copy.Config.ShouldBe(state.Config);
        copy.Items.ShouldBe(state.Items);
        copy.Theta.ShouldBe(state.Theta);
        copy.StopReason.ShouldBe(CatStopReason.ClearResult);
    }

    [Fact]
    public void The_config_is_stored_with_the_database_md_key_names()
    {
        var json = CatState.ConfigToJson(CatConfig.NclexRn).RootElement.GetRawText();

        json.ShouldBe("""{"minItems":85,"maxItems":150,"passingTheta":0}""");
    }

    /// <summary>
    /// The whole loop, as the server runs it, for a simulated candidate who earns the expected score
    /// on every question: strong and weak candidates stop at the minimum, a borderline one goes to
    /// the maximum.
    /// </summary>
    [Theory]
    [InlineData(1.5, 85, true)]
    [InlineData(-1.5, 85, false)]
    [InlineData(0.0, 150, true)]
    public void A_simulated_exam_stops_where_the_rules_say(double trueTheta, int expectedLength, bool expectedPass)
    {
        var plan = new[] { new LeafWeight(2, 18m), new LeafWeight(3, 13m), new LeafWeight(4, 9m) };
        var pool = plan
            .SelectMany(l => new[] { -1.0, 0.0, 1.0 }.SelectMany(b => Enumerable.Range(0, 80).Select(_ => new CatCandidate(Guid.NewGuid(), Guid.NewGuid(), b, l.ClientNeedsId))))
            .ToList();
        var random = new Random(42);
        var config = CatConfig.NclexRn;

        var state = CatState.Begin(config, ToItem(ItemSelector.Choose(pool, 0, new Dictionary<short, int>(), plan, random)!));
        CatStopReason? stop = null;
        while (stop is null)
        {
            var current = state.Items[^1];
            state = state.RecordAnswer(RaschModel.Probability(trueTheta, current.Difficulty));
            stop = StoppingRule.AfterAnswer(config, state.Estimate, state.AnsweredCount);
            if (stop is null)
            {
                var given = state.GivenQuestionIds;
                var next = ItemSelector.Choose([.. pool.Where(c => !given.Contains(c.QuestionId))], state.Theta, state.ContentCounts, plan, random)!;
                state = state.Give(ToItem(next));
            }
        }

        state.AnsweredCount.ShouldBe(expectedLength);
        state.Passed.ShouldBe(expectedPass);
    }

    private static CatStateItem ToItem(CatCandidate candidate) =>
        new(candidate.QuestionId, candidate.Difficulty, candidate.ClientNeedsId);

    private static CatStateItem Item(short leaf = 2) => new(Guid.NewGuid(), 0, leaf);
}
