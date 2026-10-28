using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Attempts;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Attempts;

/// <summary>
/// Grading, results and review (architecture.md 5.5) through the real API. Each test imports a set
/// of three items: a multiple-choice item (SECE.MOC, key b), a SATA item (SECE.SIC, key a, b, e)
/// and a multiple-choice item tagged to the top-level category PSI.
/// </summary>
public sealed class GradingTests(ApiFactory factory)
{
    private const string Device = "c-1";

    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));

    // Tutor mode

    [Fact]
    public async Task Submitting_a_tutor_item_locks_it_and_reveals_its_key_grade_and_rationale()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "tutor" });
        await _attempts.SaveAsync(user, attemptId, 2, Device, Sata("a", "b", "d"), 1);

        var submit = await AttemptsApi.JsonOkAsync(await _attempts.SubmitItemAsync(user, attemptId, 2, Device));
        var save = await _attempts.SaveAsync(user, attemptId, 2, Device, Sata("a", "b", "e"), 2);

        var reveal = submit.GetProperty("reveal");
        reveal.GetProperty("answerKey").GetProperty("correct").EnumerateArray().Select(e => e.GetString()).ShouldBe(["a", "b", "e"]);
        var grade = reveal.GetProperty("grade");
        (grade.GetProperty("points").GetDecimal(), grade.GetProperty("maxPoints").GetDecimal(), grade.GetProperty("isCorrect").GetBoolean())
            .ShouldBe((1m, 3m, false));
        Detail(grade).ShouldBe(["a:1", "b:1", "c:0", "d:-1", "e:0 missed"], ignoreOrder: true);
        reveal.GetProperty("rationale").GetProperty("perOption").GetProperty("d").GetString().ShouldBe("Because d");
        reveal.GetProperty("educationalObjective").GetString().ShouldBe("Prioritise safety.");
        var tags = reveal.GetProperty("tags");
        (tags.GetProperty("clientNeeds").GetString(), tags.GetProperty("bodySystem").GetString(), tags.GetProperty("cjmmSkill").GetString())
            .ShouldBe(("SECE.SIC", "respiratory", "take_action"));
        await save.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ITEM_LOCKED");
    }

    [Fact]
    public async Task A_locked_tutor_item_shows_its_reveal_and_the_others_stay_hidden()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "tutor" });
        await _attempts.SubmitItemAsync(user, attemptId, 1, Device, new { response = Mc("b"), clientSeq = 1 });

        var locked = await _attempts.GetItemOkAsync(user, attemptId, 1);
        var next = await _attempts.GetItemOkAsync(user, attemptId, 2);

        locked.GetProperty("state").GetString().ShouldBe("locked");
        locked.GetProperty("reveal").GetProperty("grade").GetProperty("isCorrect").GetBoolean().ShouldBeTrue();
        next.GetProperty("reveal").ValueKind.ShouldBe(JsonValueKind.Null);
        next.GetRawText().ShouldNotContain("Because", Case.Sensitive);
    }

    [Fact]
    public async Task The_answer_sent_with_an_item_submit_is_saved_before_the_item_locks()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "tutor" });
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("a"), 1);

        var submit = await AttemptsApi.JsonOkAsync(
            await _attempts.SubmitItemAsync(user, attemptId, 1, Device, new { response = Mc("b"), clientSeq = 2 }));

        submit.GetProperty("reveal").GetProperty("grade").GetProperty("points").GetDecimal().ShouldBe(1m);
        (await _attempts.GetItemOkAsync(user, attemptId, 1)).GetProperty("response").GetProperty("selected").GetString().ShouldBe("b");
    }

    [Fact]
    public async Task Submitting_a_locked_item_again_changes_nothing_and_returns_the_same_reveal()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "tutor" });
        var first = await AttemptsApi.JsonOkAsync(
            await _attempts.SubmitItemAsync(user, attemptId, 1, Device, new { response = Mc("a"), clientSeq = 1 }));

        var again = await AttemptsApi.JsonOkAsync(
            await _attempts.SubmitItemAsync(user, attemptId, 1, Device, new { response = Mc("b"), clientSeq = 2 }));

        again.GetProperty("reveal").GetRawText().ShouldBe(first.GetProperty("reveal").GetRawText());
        (await _attempts.GetItemOkAsync(user, attemptId, 1)).GetProperty("response").GetProperty("selected").GetString().ShouldBe("a");
    }

    [Fact]
    public async Task An_item_submit_with_an_answer_but_no_client_seq_is_a_validation_error()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "tutor" });

        var submit = await _attempts.SubmitItemAsync(user, attemptId, 1, Device, new { response = Mc("b") });

        (await submit.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().ShouldContain(("clientSeq", "REQUIRED"));
    }

    [Fact]
    public async Task The_final_submit_of_a_tutor_attempt_keeps_the_grades_already_given()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "tutor" });
        await _attempts.SubmitItemAsync(user, attemptId, 1, Device, new { response = Mc("b"), clientSeq = 1 });
        var firstGrade = (await ItemsAsync(attemptId))[0];

        await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(user, attemptId, Device));

        var items = await ItemsAsync(attemptId);
        (items[0].PointsEarned, items[0].GradedAt).ShouldBe((firstGrade.PointsEarned, firstGrade.GradedAt));
        items.ShouldAllBe(i => i.GradedAt != null);
    }

    // Exam mode

    [Fact]
    public async Task Exam_mode_refuses_item_submits_and_reveals_nothing_before_the_final_submit()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam" });
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("b"), 1);

        var itemSubmit = await _attempts.SubmitItemAsync(user, attemptId, 1, Device);
        var before = await _attempts.GetItemOkAsync(user, attemptId, 1);
        var review = await _attempts.ReviewAsync(user, attemptId);
        await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(user, attemptId, Device));
        var after = await _attempts.GetItemOkAsync(user, attemptId, 1);

        await itemSubmit.ShouldBeProblemAsync(HttpStatusCode.Conflict, "TUTOR_MODE_ONLY");
        before.GetProperty("reveal").ValueKind.ShouldBe(JsonValueKind.Null);
        await review.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_NOT_ACTIVE");
        after.GetProperty("reveal").GetProperty("answerKey").GetProperty("correct").GetString().ShouldBe("b");
    }

    // Final submit and results

    [Fact]
    public async Task Submit_grades_every_item_and_returns_the_score_with_partial_credit()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam", passThreshold = 0.65 });
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("b"), 1);
        await _attempts.SaveAsync(user, attemptId, 2, Device, Sata("a", "b", "d"), 2);

        var submit = await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(user, attemptId, Device));

        submit.GetProperty("status").GetString().ShouldBe("submitted");
        submit.GetProperty("submitReason").GetString().ShouldBe("user");
        var score = submit.GetProperty("result").GetProperty("score");
        // 1/1 + 1/3 (a and b right, d wrong) + 0/1 for the blank = 2/5.
        (score.GetProperty("pointsEarned").GetDecimal(), score.GetProperty("maxPoints").GetDecimal(), score.GetProperty("pct").GetDecimal())
            .ShouldBe((2m, 5m, 0.4m));
        (score.GetProperty("itemsCorrect").GetInt32(), score.GetProperty("itemsAnswered").GetInt32(), score.GetProperty("itemsTotal").GetInt32())
            .ShouldBe((1, 2, 3));
        var readiness = submit.GetProperty("result").GetProperty("readiness");
        (readiness.GetProperty("passed").GetBoolean(), readiness.GetProperty("label").GetString()).ShouldBe((false, "needs_more_practice"));
    }

    [Fact]
    public async Task The_result_page_reads_the_score_breakdown_and_weakest_areas()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam", passThreshold = 0.65 });
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("b"), 1);
        await _attempts.SaveAsync(user, attemptId, 2, Device, Sata("a", "b", "d"), 2);
        await _attempts.SubmitAsync(user, attemptId, Device);

        var result = await AttemptsApi.JsonOkAsync(await _attempts.ResultAsync(user, attemptId));

        result.GetProperty("title").GetString().ShouldStartWith("Graded ");
        result.GetProperty("score").GetProperty("pct").GetDecimal().ShouldBe(0.4m);
        result.GetProperty("readiness").GetProperty("disclaimer").GetString()
            .ShouldBe("This is an estimate based on your practice score, not a prediction of your NCLEX result.");
        var breakdown = result.GetProperty("breakdown");
        Rows(breakdown, "client_needs").ShouldBe(["PSI Psychosocial Integrity 1/0 0/1", "SECE.MOC Management of Care 1/1 1/1", "SECE.SIC Safety and Infection Control 1/0 1/3"]);
        Rows(breakdown, "client_needs_parent").ShouldBe(["PSI Psychosocial Integrity 1/0 0/1", "SECE Safe & Effective Care Environment 2/1 2/4"]);
        Rows(breakdown, "item_type").ShouldBe(["multiple_choice Multiple Choice 2/1 1/2", "sata Select All That Apply 1/0 1/3"]);
        Rows(breakdown, "cjmm_skill").ShouldBe(["take_action Take action 1/0 1/3"]);
        Rows(breakdown, "body_system").Count.ShouldBe(3);
        Rows(breakdown, "difficulty").ShouldBe(["easy Easy 2/1 1/2", "moderate Moderate 1/0 1/3"]);
        result.GetProperty("weakest").EnumerateArray().Select(w => $"{w.GetProperty("dimension").GetString()}:{w.GetProperty("key").GetString()}")
            .ShouldBe(["client_needs:PSI", "client_needs:SECE.SIC", "client_needs:SECE.MOC"]);
    }

    [Fact]
    public async Task Reaching_the_threshold_is_likely_ready_and_a_set_without_one_has_no_estimate()
    {
        var (passUser, passAttempt, _) = await OpenedAsync(new { revealMode = "exam", passThreshold = 0.4 });
        await _attempts.SaveAsync(passUser, passAttempt, 1, Device, Mc("b"), 1);
        await _attempts.SaveAsync(passUser, passAttempt, 2, Device, Sata("a", "b", "d"), 2);
        var (plainUser, plainAttempt, _) = await OpenedAsync(new { revealMode = "exam" });

        var passed = await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(passUser, passAttempt, Device));
        var plain = await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(plainUser, plainAttempt, Device));

        var readiness = passed.GetProperty("result").GetProperty("readiness");
        (readiness.GetProperty("passed").GetBoolean(), readiness.GetProperty("label").GetString()).ShouldBe((true, "likely_ready"));
        plain.GetProperty("result").GetProperty("readiness").ValueKind.ShouldBe(JsonValueKind.Null);
        var stored = await ResultRowAsync(plainAttempt);
        (stored.Passed, stored.ReadinessLabel, stored.PassThreshold).ShouldBe((null, null, null));
    }

    [Fact]
    public async Task Answers_sent_with_submit_are_saved_before_grading()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam" });
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("a"), 1);

        var submit = await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(user, attemptId, Device, new
        {
            finalResponses = new object[]
            {
                new { position = 1, response = Mc("b"), clientSeq = 2 },
                // Older than the stored answer, so it is ignored like a stale autosave.
                new { position = 1, response = Mc("c"), clientSeq = 1 },
                new { position = 3, response = Mc("b"), clientSeq = 1 },
            },
        }));

        submit.GetProperty("result").GetProperty("score").GetProperty("pointsEarned").GetDecimal().ShouldBe(2m);
    }

    [Fact]
    public async Task A_final_answer_that_does_not_fit_its_item_fails_the_submit_and_changes_nothing()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam" });

        var submit = await _attempts.SubmitAsync(user, attemptId, Device, new
        {
            finalResponses = new[] { new { position = 1, response = Mc("z"), clientSeq = 1 } },
        });

        await submit.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        (await _attempts.GetOkAsync(user, attemptId)).Status.ShouldBe(AttemptStatus.InProgress);
    }

    [Fact]
    public async Task Submitting_twice_returns_the_same_result_and_writes_it_once()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam" });
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("b"), 1);

        // Both at once: the row lock makes the second wait and then find the attempt submitted.
        var responses = await Task.WhenAll(
            _attempts.SubmitAsync(user, attemptId, Device),
            _attempts.SubmitAsync(user, attemptId, Device));
        // Any device may ask again: an already-submitted attempt needs no lease.
        var later = await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(user, attemptId, "another-device"));

        var bodies = await Task.WhenAll(responses.Select(AttemptsApi.JsonOkAsync));
        bodies[1].GetRawText().ShouldBe(bodies[0].GetRawText());
        later.GetRawText().ShouldBe(bodies[0].GetRawText());
        (await CountAsync("attempt_results", attemptId)).ShouldBe(1);
        var breakdown = (await AttemptsApi.JsonOkAsync(await _attempts.ResultAsync(user, attemptId))).GetProperty("breakdown");
        var breakdownRows = breakdown.EnumerateObject().Sum(dimension => dimension.Value.GetArrayLength());
        (await CountAsync("attempt_category_scores", attemptId)).ShouldBe(breakdownRows);
    }

    [Fact]
    public async Task Submit_needs_the_lease_and_an_active_attempt()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam" });
        var (pausedUser, pausedId, _) = await OpenedAsync(new { revealMode = "exam" });
        await _attempts.PauseAsync(pausedUser, pausedId, Device, new { reason = "save_exit" });

        var otherDevice = await _attempts.SubmitAsync(user, attemptId, "phone");
        var paused = await _attempts.SubmitAsync(pausedUser, pausedId, Device);
        var noHeader = await _attempts.SubmitAsync(user, attemptId, clientId: null);

        await otherDevice.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_OPENED_ELSEWHERE");
        await paused.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_NOT_ACTIVE");
        await noHeader.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task A_draft_has_no_result_and_someone_elses_attempt_is_not_found()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam" });
        var stranger = (await _attempts.VerifiedUserAsync()).AccessToken;

        var draft = await _attempts.ResultAsync(user, attemptId);
        await _attempts.SubmitAsync(user, attemptId, Device);
        var strangerResult = await _attempts.ResultAsync(stranger, attemptId);
        var strangerReview = await _attempts.ReviewAsync(stranger, attemptId);
        var strangerSubmit = await _attempts.SubmitAsync(stranger, attemptId, Device);

        await draft.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_NOT_ACTIVE");
        await strangerResult.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        await strangerReview.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        await strangerSubmit.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    // Question history

    [Fact]
    public async Task Submit_records_the_questions_the_candidate_saw_for_the_builder_pools()
    {
        var (user, attemptId, questions) = await OpenedAsync(new { revealMode = "exam" });
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("b"), 1);
        await _attempts.NavigateAsync(user, attemptId, Device, 2);
        await _attempts.SubmitAsync(user, attemptId, Device);

        var history = await HistoryAsync(questions);

        // Item 2 was seen but left blank, so it counts as incorrect; item 3 was never reached.
        history.Select(h => (h.QuestionId, h.TimesSeen, h.LastCorrect))
            .ShouldBe([(questions[0], 1, (bool?)true), (questions[1], 1, (bool?)false)], ignoreOrder: true);
    }

    [Fact]
    public async Task Seeing_a_question_again_updates_its_history()
    {
        var (setId, questions) = await ImportSetAsync(new { revealMode = "exam" });
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var first = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, first.Id, Device);
        await _attempts.SaveAsync(user, first.Id, 1, Device, Mc("a"), 1);
        await _attempts.SubmitAsync(user, first.Id, Device);

        var second = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, second.Id, Device);
        await _attempts.SaveAsync(user, second.Id, 1, Device, Mc("b"), 1);
        await _attempts.SubmitAsync(user, second.Id, Device);

        var row = (await HistoryAsync(questions)).Single(h => h.QuestionId == questions[0]);
        (row.TimesSeen, row.LastCorrect).ShouldBe((2, (bool?)true));
    }

    // Review

    [Fact]
    public async Task Review_lists_every_item_with_its_reveal_and_filters_incorrect_and_flagged()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "exam" });
        await _attempts.SaveAsync(user, attemptId, 1, Device, Mc("b"), 1);
        await _attempts.MarkAsync(user, attemptId, 1, Device, new { flagged = true });
        await _attempts.SaveAsync(user, attemptId, 2, Device, Sata("a", "b", "d"), 2);
        await _attempts.SubmitAsync(user, attemptId, Device);

        var all = await ReviewPositionsAsync(user, attemptId, "");
        var incorrect = await ReviewPositionsAsync(user, attemptId, "?filter=incorrect");
        var flagged = await ReviewPositionsAsync(user, attemptId, "?filter=flagged");
        var page = await AttemptsApi.JsonOkAsync(await _attempts.ReviewAsync(user, attemptId, "?limit=2"));
        var rest = await ReviewPositionsAsync(user, attemptId, $"?limit=2&cursor={page.GetProperty("nextCursor").GetString()}");

        all.ShouldBe([1, 2, 3]);
        // Partial credit and blanks count as incorrect.
        incorrect.ShouldBe([2, 3]);
        flagged.ShouldBe([1]);
        page.GetProperty("items").GetArrayLength().ShouldBe(2);
        rest.ShouldBe([3]);
        var review = await AttemptsApi.JsonOkAsync(await _attempts.ReviewAsync(user, attemptId));
        review.GetProperty("items").EnumerateArray().ShouldAllBe(i => i.GetProperty("reveal").ValueKind == JsonValueKind.Object);
        review.GetProperty("nextCursor").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task A_tutor_attempt_in_progress_reviews_only_its_locked_items()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "tutor" });
        await _attempts.SubmitItemAsync(user, attemptId, 2, Device, new { response = Sata("a"), clientSeq = 1 });

        (await ReviewPositionsAsync(user, attemptId, "")).ShouldBe([2]);
        (await ReviewPositionsAsync(user, attemptId, "?filter=incorrect")).ShouldBe([2]);
    }

    [Fact]
    public async Task A_bad_review_filter_is_a_validation_error()
    {
        var (user, attemptId, _) = await OpenedAsync(new { revealMode = "tutor" });

        var review = await _attempts.ReviewAsync(user, attemptId, "?filter=everything");

        await review.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    private static object Mc(string selected) => new { selected };

    private static object Sata(params string[] selected) => new { selected };

    private static IEnumerable<string> Detail(JsonElement grade) =>
        grade.GetProperty("detail").EnumerateArray().Select(d =>
            $"{d.GetProperty("target").GetString()}:{d.GetProperty("delta").GetInt32()}"
            + (d.GetProperty("missed").GetBoolean() ? " missed" : ""));

    private static List<string> Rows(JsonElement breakdown, string dimension) =>
        [.. breakdown.GetProperty(dimension).EnumerateArray().Select(r =>
            $"{r.GetProperty("key").GetString()} {r.GetProperty("name").GetString()} "
            + $"{r.GetProperty("items").GetInt32()}/{r.GetProperty("correct").GetInt32()} "
            + $"{r.GetProperty("pointsEarned").GetDecimal():0.##}/{r.GetProperty("maxPoints").GetDecimal():0.##}")];

    private async Task<List<int>> ReviewPositionsAsync(string token, Guid attemptId, string query)
    {
        var page = await AttemptsApi.JsonOkAsync(await _attempts.ReviewAsync(token, attemptId, query));
        return [.. page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("position").GetInt32())];
    }

    private async Task<(string Token, Guid AttemptId, List<Guid> Questions)> OpenedAsync(object settings)
    {
        var (setId, questions) = await ImportSetAsync(settings);
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, attempt.Id, Device);
        return (user, attempt.Id, questions);
    }

    private async Task<(Guid SetId, List<Guid> Questions)> ImportSetAsync(object settings)
    {
        var (questions, setId) = (CatalogContent.NewIds(3), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.FileWith(
            "RN",
            [
                CatalogContent.Question(questions[0], "SECE.MOC", "fundamentals"),
                CatalogContent.SataQuestion(questions[1]),
                CatalogContent.Question(questions[2], "PSI", "mental-health"),
            ],
            testSets: [CatalogContent.Set(setId, $"Graded {setId:N}", questions, settings)]));
        return (setId, questions);
    }

    private async Task<List<AttemptItem>> ItemsAsync(Guid attemptId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().AttemptItems.AsNoTracking()
            .Where(i => i.AttemptId == attemptId)
            .OrderBy(i => i.Position)
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    private async Task<AttemptResult> ResultRowAsync(Guid attemptId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().AttemptResults.AsNoTracking()
            .SingleAsync(r => r.AttemptId == attemptId, TestContext.Current.CancellationToken);
    }

    private async Task<List<UserQuestionHistory>> HistoryAsync(List<Guid> questions)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().UserQuestionHistory.AsNoTracking()
            .Where(h => questions.Contains(h.QuestionId))
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    private async Task<int> CountAsync(string table, Guid attemptId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var sql = table == "attempt_results"
            ? (FormattableString)$"SELECT count(*)::int AS \"Value\" FROM attempt_results WHERE attempt_id = {attemptId}"
            : $"SELECT count(*)::int AS \"Value\" FROM attempt_category_scores WHERE attempt_id = {attemptId}";
        return await db.Database.SqlQuery<int>(sql).SingleAsync(TestContext.Current.CancellationToken);
    }
}
