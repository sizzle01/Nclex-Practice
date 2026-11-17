using System.Text.Json;
using System.Text.Json.Serialization;

namespace Nclex.Domain.Cat;

/// <summary>A question given in an adaptive attempt, in the order given.</summary>
/// <param name="QuestionId">The question (not a version), so it is never given twice.</param>
/// <param name="Difficulty">Its difficulty b when it was given, in logits.</param>
/// <param name="ClientNeedsId">Its Client Needs leaf, for content balancing.</param>
/// <param name="Score">Share of its points earned once answered; null while it is the current question.</param>
public sealed record CatStateItem(Guid QuestionId, double Difficulty, short ClientNeedsId, double? Score = null);

/// <summary>
/// An adaptive attempt's progress (<c>attempts.cat_state</c>, architecture.md 5.8): the rules it
/// runs under, the current ability estimate, and every question given. Immutable: each change
/// returns a new state, which the attempt stores.
/// </summary>
/// <param name="Config">The set's rules, copied when the attempt started.</param>
/// <param name="Theta">The ability estimate after the answers so far.</param>
/// <param name="StandardError">The estimate's standard error.</param>
/// <param name="Items">Every question given, in order; the last is the current one until it is answered.</param>
/// <param name="StopReason">Why the exam ended; null while it runs, or when time ran out.</param>
public sealed record CatState(
    CatConfig Config,
    double Theta,
    double StandardError,
    IReadOnlyList<CatStateItem> Items,
    CatStopReason? StopReason = null)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>A new adaptive attempt: the prior estimate and its first question.</summary>
    public static CatState Begin(CatConfig config, CatStateItem first) =>
        new(config, AbilityEstimate.Prior.Theta, AbilityEstimate.Prior.StandardError, [first with { Score = null }]);

    /// <summary>The current estimate.</summary>
    [JsonIgnore]
    public AbilityEstimate Estimate => new(Theta, StandardError);

    /// <summary>Questions answered so far.</summary>
    [JsonIgnore]
    public int AnsweredCount => Items.Count(i => i.Score is not null);

    /// <summary>Questions given per Client Needs leaf, the current one included.</summary>
    [JsonIgnore]
    public IReadOnlyDictionary<short, int> ContentCounts =>
        Items.GroupBy(i => i.ClientNeedsId).ToDictionary(g => g.Key, g => g.Count());

    /// <summary>Every question given, so none is given twice.</summary>
    [JsonIgnore]
    public IReadOnlySet<Guid> GivenQuestionIds => Items.Select(i => i.QuestionId).ToHashSet();

    /// <summary>Whether the exam passes if it ends now (StoppingRule.Passed).</summary>
    [JsonIgnore]
    public bool Passed => StoppingRule.Passed(Config, Estimate, AnsweredCount);

    /// <summary>
    /// Records the score of the current (last) question and re-estimates ability from every answer.
    /// </summary>
    public CatState RecordAnswer(double score)
    {
        if (Items.Count == 0 || Items[^1].Score is not null)
        {
            throw new InvalidOperationException("There is no unanswered current question to score.");
        }

        List<CatStateItem> items = [.. Items.Take(Items.Count - 1), Items[^1] with { Score = Math.Clamp(score, 0.0, 1.0) }];
        var estimate = AbilityEstimator.Estimate([.. items.Select(i => new ScoredResponse(i.Difficulty, i.Score!.Value))]);
        return this with { Items = items, Theta = estimate.Theta, StandardError = estimate.StandardError };
    }

    /// <summary>Adds the next question, which becomes the current one.</summary>
    public CatState Give(CatStateItem next)
    {
        if (Items.Count > 0 && Items[^1].Score is null)
        {
            throw new InvalidOperationException("The current question must be answered before the next is given.");
        }

        if (GivenQuestionIds.Contains(next.QuestionId))
        {
            throw new InvalidOperationException("A question is never given twice in one adaptive attempt.");
        }

        return this with { Items = [.. Items, next with { Score = null }] };
    }

    /// <summary>Marks the exam as ended for <paramref name="reason"/>.</summary>
    public CatState Stop(CatStopReason reason) => this with { StopReason = reason };

    /// <summary>The state as stored in <c>attempts.cat_state</c>.</summary>
    public JsonDocument ToJson() => JsonSerializer.SerializeToDocument(this, JsonOptions);

    /// <summary>Reads the state stored by <see cref="ToJson"/>.</summary>
    public static CatState FromJson(JsonDocument json) =>
        json.Deserialize<CatState>(JsonOptions) ?? throw new InvalidOperationException("The stored CAT state is empty.");

    /// <summary>Reads a CAT config stored in <c>test_sets.cat_config</c>.</summary>
    public static CatConfig ConfigFromJson(JsonDocument json) =>
        json.Deserialize<CatConfig>(JsonOptions) ?? throw new InvalidOperationException("The stored CAT config is empty.");

    /// <summary>A CAT config as stored in <c>test_sets.cat_config</c>: <c>{minItems, maxItems, passingTheta}</c>.</summary>
    public static JsonDocument ConfigToJson(CatConfig config) => JsonSerializer.SerializeToDocument(config, JsonOptions);
}
