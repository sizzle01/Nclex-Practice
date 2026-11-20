using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Attempts;

/// <summary>The clinical judgment section of GET /attempts/{id}/result (F25).</summary>
public sealed class ClinicalJudgmentResultTests(ApiFactory factory)
{
    private const string Device = "cj-1";

    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));

    [Fact]
    public async Task The_result_shows_each_step_in_model_order_and_the_two_halves()
    {
        // Served order: evaluate outcomes, recognize cues | take action, take action.
        var (user, attemptId) = await OpenedAsync("evaluate_outcomes", "recognize_cues", "take_action", "take_action");
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("b"), 1);
        await _attempts.SaveAsync(user, attemptId, 2, Device, Mc("b"), 2);
        await _attempts.SaveAsync(user, attemptId, 3, Device, Mc("a"), 3);
        // Item 4 is left blank: it counts as 0 of 1, as in the score.
        await _attempts.SubmitAsync(user, attemptId, Device);

        var result = await AttemptsApi.JsonOkAsync(await _attempts.ResultAsync(user, attemptId));

        var judgment = result.GetProperty("clinicalJudgment");
        Steps(judgment.GetProperty("steps")).ShouldBe(["recognize_cues 1/1", "take_action 0/2", "evaluate_outcomes 1/1"]);
        judgment.GetProperty("steps")[0].GetProperty("name").GetString().ShouldBe("Recognize cues");
        var first = judgment.GetProperty("firstHalf");
        var second = judgment.GetProperty("secondHalf");
        (first.GetProperty("items").GetInt32(), first.GetProperty("pct").GetDecimal()).ShouldBe((2, 1m));
        (second.GetProperty("items").GetInt32(), second.GetProperty("pct").GetDecimal()).ShouldBe((2, 0m));
        Steps(second.GetProperty("steps")).ShouldBe(["take_action 0/2"]);
        second.GetProperty("avgTimeMs").ValueKind.ShouldBe(JsonValueKind.Number);
        // Two items a half are too few to call it a drop.
        judgment.GetProperty("secondHalfDrop").GetBoolean().ShouldBeFalse();
    }

    private static List<string> Steps(JsonElement steps) =>
        [.. steps.EnumerateArray().Select(s =>
            $"{s.GetProperty("key").GetString()} {s.GetProperty("pointsEarned").GetDecimal():0}/{s.GetProperty("maxPoints").GetDecimal():0}")];

    private static object Mc(string selected) => new { selected };

    // An exam-mode set of multiple-choice questions (key "b"), one per given CJMM step, in that order.
    private async Task<(string Token, Guid AttemptId)> OpenedAsync(params string[] steps)
    {
        var ids = CatalogContent.NewIds(steps.Length);
        var setId = Guid.NewGuid();
        var questions = ids.Select((id, i) =>
        {
            var question = CatalogContent.Question(id);
            question["cjmmSkill"] = steps[i];
            return question;
        }).ToList();
        await _content.ImportOkAsync(CatalogContent.FileWith(
            "RN",
            questions,
            testSets: [CatalogContent.Set(setId, $"Judgment {setId:N}", ids, new { mode = "challenge", revealMode = "exam" })]));

        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, attempt.Id, Device);
        return (user, attempt.Id);
    }
}
