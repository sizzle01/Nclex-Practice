using System.Net;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Attempts;

/// <summary>
/// A bowtie item (F20) through the real API: imported, shown without its key, answered, graded one
/// point per slot and revealed. The question comes from <see cref="CatalogContent.BowtieQuestion"/>.
/// </summary>
public sealed class BowtieAttemptTests(ApiFactory factory)
{
    private const string Device = "c-1";

    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));

    [Fact]
    public async Task The_item_shows_all_three_lists_and_nothing_of_the_key()
    {
        var (user, attemptId) = await OpenedAsync("tutor");

        var item = await _attempts.GetItemOkAsync(user, attemptId, 1);

        item.GetProperty("itemType").GetString().ShouldBe("bowtie");
        var content = item.GetProperty("content");
        (Ids(content, "condition").Count, Ids(content, "actions").Count, Ids(content, "parameters").Count).ShouldBe((4, 5, 5));
        Ids(content, "actions").ShouldBe(["a1", "a2", "a3", "a4", "a5"], ignoreOrder: true);
        item.GetProperty("reveal").ValueKind.ShouldBe(JsonValueKind.Null);
        var raw = item.GetRawText();
        raw.ShouldNotContain("answerKey", Case.Insensitive);
        raw.ShouldNotContain("Because c2");
    }

    [Fact]
    public async Task Submitting_in_tutor_mode_grades_one_point_per_slot_and_reveals_the_key()
    {
        var (user, attemptId) = await OpenedAsync("tutor");
        await _attempts.SaveAsync(user, attemptId, 1, Device, Bowtie("c2", ["a1", "a4"], ["p2"]), 1);

        var submit = await AttemptsApi.JsonOkAsync(await _attempts.SubmitItemAsync(user, attemptId, 1, Device));

        var reveal = submit.GetProperty("reveal");
        var key = reveal.GetProperty("answerKey");
        key.GetProperty("condition").GetString().ShouldBe("c2");
        key.GetProperty("actions").EnumerateArray().Select(e => e.GetString()).ShouldBe(["a1", "a3"]);
        var grade = reveal.GetProperty("grade");
        // Condition right, one action right and one wrong (no penalty), one parameter right and one slot empty.
        (grade.GetProperty("points").GetDecimal(), grade.GetProperty("maxPoints").GetDecimal(), grade.GetProperty("isCorrect").GetBoolean())
            .ShouldBe((3m, 5m, false));
        Marked(grade).ShouldBe(["c2:1", "a1:1", "a3:0 missed", "a4:0", "p2:1", "p4:0 missed"], ignoreOrder: true);
        reveal.GetProperty("rationale").GetProperty("perOption").GetProperty("p5").GetString().ShouldBe("Because p5");
    }

    [Fact]
    public async Task An_exam_submit_scores_a_full_bowtie_as_correct()
    {
        var (user, attemptId) = await OpenedAsync("exam");
        await _attempts.SaveAsync(user, attemptId, 1, Device, Bowtie("c2", ["a3", "a1"], ["p4", "p2"]), 1);

        var submit = await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(user, attemptId, Device));

        var score = submit.GetProperty("result").GetProperty("score");
        (score.GetProperty("pointsEarned").GetDecimal(), score.GetProperty("maxPoints").GetDecimal(), score.GetProperty("itemsCorrect").GetInt32())
            .ShouldBe((5m, 5m, 1));
    }

    [Fact]
    public async Task A_wing_with_three_options_or_an_option_from_another_list_is_refused()
    {
        var (user, attemptId) = await OpenedAsync("tutor");

        var tooMany = await _attempts.SaveAsync(user, attemptId, 1, Device, Bowtie("c2", ["a1", "a2", "a3"], []), 1);
        var wrongList = await _attempts.SaveAsync(user, attemptId, 1, Device, Bowtie("a1", [], []), 2);

        (await tooMany.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().ShouldContain(("response.actions", "TOO_MANY"));
        (await wrongList.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().ShouldContain(("response.condition[0]", "UNKNOWN_ID"));
    }

    private static object Bowtie(string? condition, string[] actions, string[] parameters) => new { condition, actions, parameters };

    private static List<string> Ids(JsonElement content, string list) =>
        [.. content.GetProperty(list).EnumerateArray().Select(o => o.GetProperty("id").GetString()!)];

    // The detail entries that say something: a point, a placed option or a missed one.
    private static IEnumerable<string> Marked(JsonElement grade) =>
        grade.GetProperty("detail").EnumerateArray()
            .Where(d => d.GetProperty("delta").GetInt32() != 0 || d.GetProperty("missed").GetBoolean() || d.GetProperty("selected").ValueKind == JsonValueKind.String)
            .Select(d => $"{d.GetProperty("target").GetString()}:{d.GetProperty("delta").GetInt32()}" + (d.GetProperty("missed").GetBoolean() ? " missed" : ""));

    private async Task<(string Token, Guid AttemptId)> OpenedAsync(string revealMode)
    {
        var (questionId, setId) = (Guid.NewGuid(), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.FileWith(
            "RN",
            [CatalogContent.BowtieQuestion(questionId)],
            testSets: [CatalogContent.Set(setId, $"Bowtie {setId:N}", [questionId], new { revealMode })]));
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, attempt.Id, Device);
        return (user, attempt.Id);
    }
}
