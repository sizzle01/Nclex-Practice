using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Performance;

/// <summary>GET /me/performance/clinical-judgment (F25): each CJMM step across the last five timed attempts.</summary>
public sealed class ClinicalJudgmentTrendTests(ApiFactory factory) : IAsyncLifetime
{
    private const string Device = "cjt-1";
    private const string Path = "/api/v1/me/performance/clinical-judgment";

    private readonly TimedTestHost _host = new(factory);
    private readonly CatalogContent _content = new(factory);
    private Guid _timedSet;
    private Guid _untimedSet;

    public async ValueTask InitializeAsync()
    {
        // Two multiple-choice questions (key b): take action, then evaluate outcomes.
        var ids = CatalogContent.NewIds(2);
        var questions = ids.Select((id, i) =>
        {
            var question = CatalogContent.Question(id);
            question["cjmmSkill"] = i == 0 ? "take_action" : "evaluate_outcomes";
            return question;
        }).ToList();
        (_timedSet, _untimedSet) = (Guid.NewGuid(), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.FileWith("RN", questions, testSets:
        [
            CatalogContent.Set(_timedSet, $"Timed {_timedSet:N}", ids, new { mode = "challenge", revealMode = "exam", timed = true, timeBudgetSec = 3600 }),
            CatalogContent.Set(_untimedSet, $"Untimed {_untimedSet:N}", ids, new { revealMode = "exam" }),
        ]));
    }

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    [Fact]
    public async Task Each_step_is_a_row_across_the_timed_attempts_oldest_first()
    {
        var token = (await _host.Attempts.VerifiedUserAsync()).AccessToken;
        await SubmitAsync(token, _timedSet, takeAction: "b", evaluate: "a");
        await SubmitAsync(token, _untimedSet, takeAction: "b", evaluate: "b");
        await SubmitAsync(token, _timedSet, takeAction: "a", evaluate: "b");

        var trend = await GetOkAsync(token);

        trend.GetProperty("attempts").GetArrayLength().ShouldBe(2);
        var rows = Rows(trend);
        rows.Select(r => r.Key).ShouldBe(["recognize_cues", "analyze_cues", "prioritize_hypotheses", "generate_solutions", "take_action", "evaluate_outcomes"]);
        rows.Single(r => r.Key == "take_action").Scores.ShouldBe(["1.0000", "0.0000"]);
        rows.Single(r => r.Key == "evaluate_outcomes").Scores.ShouldBe(["0.0000", "1.0000"]);
        rows.Single(r => r.Key == "recognize_cues").Scores.ShouldBe(["-", "-"]);
    }

    [Fact]
    public async Task Only_the_last_five_timed_attempts_are_shown()
    {
        var token = (await _host.Attempts.VerifiedUserAsync()).AccessToken;
        var first = await SubmitAsync(token, _timedSet, takeAction: "a", evaluate: "a");
        for (var i = 0; i < 5; i++)
        {
            await SubmitAsync(token, _timedSet, takeAction: "b", evaluate: "b");
        }

        var attempts = (await GetOkAsync(token)).GetProperty("attempts").EnumerateArray()
            .Select(a => a.GetProperty("attemptId").GetGuid()).ToList();

        attempts.Count.ShouldBe(5);
        attempts.ShouldNotContain(first);
    }

    [Fact]
    public async Task A_user_with_no_timed_attempts_gets_six_empty_rows()
    {
        var token = (await _host.Attempts.VerifiedUserAsync()).AccessToken;

        var trend = await GetOkAsync(token);

        trend.GetProperty("attempts").GetArrayLength().ShouldBe(0);
        Rows(trend).ShouldAllBe(r => r.Scores.Count == 0);
        Rows(trend).Count.ShouldBe(6);
    }

    private async Task<Guid> SubmitAsync(string token, Guid setId, string takeAction, string evaluate)
    {
        // Moves the clock on so attempts keep their order in submitted_at.
        _host.Clock.Advance(TimeSpan.FromMinutes(1));
        var attempt = await _host.Attempts.StartOkAsync(token, setId);
        await _host.Attempts.OpenOkAsync(token, attempt.Id, Device);
        await _host.Attempts.SaveAsync(token, attempt.Id, 1, Device, new { selected = takeAction }, 1);
        await _host.Attempts.SaveAsync(token, attempt.Id, 2, Device, new { selected = evaluate }, 2);
        await AttemptsApi.JsonOkAsync(await _host.Attempts.SubmitAsync(token, attempt.Id, Device));
        return attempt.Id;
    }

    private async Task<JsonElement> GetOkAsync(string token) =>
        await AttemptsApi.JsonOkAsync(await _host.Auth.SendWithTokenAsync(HttpMethod.Get, Path, token));

    private static List<(string Key, List<string> Scores)> Rows(JsonElement trend) =>
        [.. trend.GetProperty("steps").EnumerateArray().Select(s => (
            s.GetProperty("key").GetString()!,
            s.GetProperty("scores").EnumerateArray()
                .Select(v => v.ValueKind == JsonValueKind.Null ? "-" : v.GetDecimal().ToString("0.0000", System.Globalization.CultureInfo.InvariantCulture))
                .ToList()))];
}
