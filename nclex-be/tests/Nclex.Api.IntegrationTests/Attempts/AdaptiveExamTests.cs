using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Abstractions;
using Nclex.Application.Attempts;
using Nclex.Application.Attempts.Adaptive;
using Nclex.Application.Catalog;
using Nclex.Application.Content.ImportQuestions;

namespace Nclex.Api.IntegrationTests.Attempts;

/// <summary>
/// The adaptive exam (F19, architecture.md 5.8) through the real API, on a host with a manual clock
/// whose picker serves only this test's questions. Every question is multiple choice with "b"
/// correct, so a test can answer right or wrong on purpose. The sets are small (3 to 6 questions)
/// so a whole exam runs in a few requests; the NCLEX's 85 to 150 is covered in the domain tests.
/// </summary>
public sealed class AdaptiveExamTests(ApiFactory factory) : IAsyncLifetime
{
    private const int BudgetSec = 3600;

    private readonly ConcurrentDictionary<Guid, byte> _ownQuestions = new();
    private readonly CatalogContent _content = new(factory);
    private TimedTestHost _host = null!;

    private AttemptsApi Attempts => _host.Attempts;

    public ValueTask InitializeAsync()
    {
        _host = new TimedTestHost(factory, services => services.Replace(ServiceDescriptor.Scoped<AdaptivePicker>(sp =>
            new OwnQuestionsPicker(sp.GetRequiredService<IAppDbContext>(), _ownQuestions))));
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    // Catalog and start

    [Fact]
    public async Task The_catalog_shows_an_adaptive_set_with_its_length_rules()
    {
        var setId = await ImportAdaptiveSetAsync();
        var user = await Attempts.VerifiedUserAsync();

        var response = await _host.Auth.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{setId}", user.AccessToken);
        var detail = await response.Content.ReadFromJsonAsync<TestSetDetail>(AuthApi.Json, TestContext.Current.CancellationToken);

        detail!.ShouldSatisfyAllConditions(
            d => d.Kind.ShouldBe(Nclex.Domain.Entities.TestSetKind.Cat),
            d => d.Mode.ShouldBe(Nclex.Domain.Entities.TestMode.Cat),
            d => d.QuestionCount.ShouldBe(6),
            d => d.Adaptive.ShouldBe(new AdaptiveRules(3, 6)));
    }

    [Fact]
    public async Task A_timed_start_runs_the_set_budget_with_one_question()
    {
        var setId = await ImportAdaptiveSetAsync();
        var user = await Attempts.VerifiedUserAsync();

        var attempt = await StartAsync(user.AccessToken, setId, timed: true);

        attempt.ShouldSatisfyAllConditions(
            a => a.TotalItems.ShouldBe(1),
            a => a.Settings.TimeBudgetSec.ShouldBe(BudgetSec),
            a => a.Settings.AllowBackNav.ShouldBeFalse(),
            a => a.Adaptive.ShouldBe(new AdaptiveProgress(Answered: 0, MinItems: 3, MaxItems: 6, Timed: true)));
    }

    [Fact]
    public async Task An_untimed_start_has_no_exam_timer()
    {
        var setId = await ImportAdaptiveSetAsync();
        var user = await Attempts.VerifiedUserAsync();

        var attempt = await StartAsync(user.AccessToken, setId, timed: false);
        var opened = await Attempts.OpenOkAsync(user.AccessToken, attempt.Id, "c-1");

        attempt.Settings.TimeBudgetSec.ShouldBeNull();
        attempt.Adaptive!.Timed.ShouldBeFalse();
        opened.Timer.Exam.ShouldBeNull();
        _host.Jobs.Expiries.ShouldBeEmpty();
    }

    [Fact]
    public async Task Starting_again_resumes_the_draft_with_its_first_timing()
    {
        var setId = await ImportAdaptiveSetAsync();
        var user = await Attempts.VerifiedUserAsync();
        var first = await StartAsync(user.AccessToken, setId, timed: false);

        var again = await StartRawAsync(user.AccessToken, setId, timed: true);

        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        var state = await again.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken);
        state!.Id.ShouldBe(first.Id);
        state.Adaptive!.Timed.ShouldBeFalse();
    }

    [Fact]
    public async Task The_timed_choice_is_refused_for_a_set_that_is_not_adaptive()
    {
        var setId = await ImportFixedSetAsync();
        var user = await Attempts.VerifiedUserAsync();

        var response = await StartRawAsync(user.AccessToken, setId, timed: true);

        (await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().ShouldContain(("timed", "INVALID_VALUE"));
    }

    // Answering

    [Fact]
    public async Task An_answer_locks_the_question_and_serves_a_new_one_without_revealing_the_key()
    {
        var (user, attempt) = await OpenedAsync();
        var firstItem = await Attempts.GetItemOkAsync(user, attempt.Id, 1);

        var result = await AnswerAsync(user, attempt.Id, 1, correct: true);
        var secondItem = await Attempts.GetItemOkAsync(user, attempt.Id, 2);
        var answered = await Attempts.GetItemOkAsync(user, attempt.Id, 1);

        result.Ended.ShouldBeFalse();
        result.Attempt.ShouldSatisfyAllConditions(
            a => a.CurrentPosition.ShouldBe(2),
            a => a.TotalItems.ShouldBe(2),
            a => a.Adaptive!.Answered.ShouldBe(1));
        secondItem.GetProperty("questionVersionId").GetGuid().ShouldNotBe(firstItem.GetProperty("questionVersionId").GetGuid());
        answered.GetProperty("state").GetString().ShouldBe("locked");
        answered.GetProperty("reveal").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task A_retried_answer_returns_where_the_exam_is_without_answering_again()
    {
        var (user, attempt) = await OpenedAsync();
        await AnswerAsync(user, attempt.Id, 1, correct: true);

        var retry = await AnswerAsync(user, attempt.Id, 1, correct: false);

        retry.Ended.ShouldBeFalse();
        retry.Attempt.TotalItems.ShouldBe(2);
        retry.Attempt.Adaptive!.Answered.ShouldBe(1);
    }

    [Fact]
    public async Task An_answer_is_required()
    {
        var (user, attempt) = await OpenedAsync();

        var response = await PostAnswerAsync(user, attempt.Id, 1, new { response = (object?)null, clientSeq = 1 });

        (await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().ShouldContain(("response", "REQUIRED"));
    }

    [Fact]
    public async Task Navigate_and_submit_are_refused_because_the_exam_ends_on_its_own()
    {
        var (user, attempt) = await OpenedAsync();
        await AnswerAsync(user, attempt.Id, 1, correct: true);

        var back = await Attempts.NavigateAsync(user, attempt.Id, "c-1", 1);
        var submit = await Attempts.SubmitAsync(user, attempt.Id, "c-1");

        await back.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ADAPTIVE_NOT_ALLOWED");
        await submit.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ADAPTIVE_NOT_ALLOWED");
    }

    [Fact]
    public async Task Answer_is_refused_on_an_attempt_that_is_not_adaptive()
    {
        var setId = await ImportFixedSetAsync();
        var user = await Attempts.VerifiedUserAsync();
        var attempt = await Attempts.StartOkAsync(user.AccessToken, setId);
        await Attempts.OpenOkAsync(user.AccessToken, attempt.Id, "c-1");

        var response = await PostAnswerAsync(user.AccessToken, attempt.Id, 1, Answer(correct: true));

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ADAPTIVE_ONLY");
    }

    // Ending

    [Fact]
    public async Task Answering_right_every_time_ends_in_a_pass()
    {
        var (user, attempt) = await OpenedAsync();

        var (answered, last) = await AnswerUntilEndAsync(user, attempt.Id, correct: true);
        var result = await ResultAsync(user, attempt.Id);

        last.Attempt.Status.ShouldBe(Nclex.Domain.Entities.AttemptStatus.Submitted);
        answered.ShouldBeInRange(3, 6);
        result.GetProperty("submitReason").GetString().ShouldBe("cat_stop");
        var adaptive = result.GetProperty("adaptive");
        adaptive.GetProperty("passed").GetBoolean().ShouldBeTrue();
        adaptive.GetProperty("itemsAnswered").GetInt32().ShouldBe(answered);
        adaptive.GetProperty("timed").GetBoolean().ShouldBeTrue();
        result.GetProperty("readiness").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task Answering_wrong_every_time_ends_in_a_fail()
    {
        var (user, attempt) = await OpenedAsync();

        var (answered, _) = await AnswerUntilEndAsync(user, attempt.Id, correct: false);
        var result = await ResultAsync(user, attempt.Id);

        answered.ShouldBeInRange(3, 6);
        result.GetProperty("adaptive").GetProperty("passed").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task A_clear_result_stops_the_exam_at_the_minimum()
    {
        // A passing standard far below any estimate makes the result clear at once, so the exam
        // stops as soon as the minimum is reached.
        var (user, attempt) = await OpenedAsync(passingTheta: -3);

        var (answered, _) = await AnswerUntilEndAsync(user, attempt.Id, correct: true);
        var adaptive = (await ResultAsync(user, attempt.Id)).GetProperty("adaptive");

        answered.ShouldBe(3);
        adaptive.GetProperty("stopReason").GetString().ShouldBe("clear_result");
        adaptive.GetProperty("passed").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public async Task An_uncertain_result_runs_to_the_maximum()
    {
        // Alternating right and wrong keeps θ near the standard, so the interval never clears it.
        var (user, attempt) = await OpenedAsync();

        var answered = 0;
        AnswerAdaptiveItemResult last;
        do
        {
            answered++;
            last = await AnswerAsync(user, attempt.Id, answered, correct: answered % 2 == 1);
        }
        while (!last.Ended);
        var adaptive = (await ResultAsync(user, attempt.Id)).GetProperty("adaptive");

        answered.ShouldBe(6);
        adaptive.GetProperty("stopReason").GetString().ShouldBe("max_items");
    }

    [Fact]
    public async Task Running_out_of_time_before_the_minimum_is_a_fail()
    {
        var (user, attempt) = await OpenedAsync();
        await AnswerAsync(user, attempt.Id, 1, correct: true);

        _host.Clock.Advance(TimeSpan.FromSeconds(BudgetSec + 3));
        var late = await PostAnswerAsync(user, attempt.Id, 2, Answer(correct: true));
        var result = await ResultAsync(user, attempt.Id);

        await late.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_TIME_EXPIRED");
        result.GetProperty("submitReason").GetString().ShouldBe("time_expired");
        var adaptive = result.GetProperty("adaptive");
        adaptive.GetProperty("stopReason").GetString().ShouldBe("time_expired");
        adaptive.GetProperty("itemsAnswered").GetInt32().ShouldBe(1);
        adaptive.GetProperty("passed").GetBoolean().ShouldBeFalse();
    }

    [Fact]
    public async Task Once_ended_the_answers_and_rationales_can_be_reviewed()
    {
        var (user, attempt) = await OpenedAsync();
        await AnswerUntilEndAsync(user, attempt.Id, correct: true);

        var review = await AttemptsApi.JsonOkAsync(await Attempts.ReviewAsync(user, attempt.Id));

        review.GetProperty("items")[0].GetProperty("reveal").GetProperty("answerKey").GetProperty("correct").GetString().ShouldBe("b");
    }

    // Import

    [Fact]
    public async Task An_unpublished_set_leaves_the_catalog()
    {
        var questions = CatalogContent.NewIds(2);
        var setId = Guid.NewGuid();
        await _content.ImportOkAsync(FileWithQuestions(questions, CatalogContent.Set(setId, "Old challenge", questions)));

        // Re-importing the same questions leaves them unchanged; only the set is hidden.
        await _content.ImportOkAsync(FileWithQuestions(questions, CatalogContent.Set(setId, "Old challenge", questions, new { isPublished = false })));
        var user = await Attempts.VerifiedUserAsync();
        var response = await _host.Auth.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{setId}", user.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task An_adaptive_set_that_lists_questions_or_lacks_rules_is_refused()
    {
        var questions = CatalogContent.NewIds(1);
        var withQuestions = AdaptiveSet(Guid.NewGuid(), passingTheta: 0);
        withQuestions["questionIds"] = questions;
        var withoutRules = AdaptiveSet(Guid.NewGuid(), passingTheta: 0);
        withoutRules.Remove("catConfig");

        var result = await _content.ImportAsync(FileWithQuestions(questions, withQuestions, withoutRules));

        result.Issues.Select(i => i.Path).ShouldBe(["testSets[0].questionIds", "testSets[1].catConfig"], ignoreOrder: true);
    }

    // Helpers

    /// <summary>
    /// The importer refuses a file without questions, so every file carries fresh ones, which this
    /// test's picker may then serve.
    /// </summary>
    private QuestionImportFile FileWithQuestions(List<Guid> ids, params object[] testSets)
    {
        foreach (var id in ids)
        {
            _ownQuestions[id] = 0;
        }

        return CatalogContent.FileWith("RN", ids.Select(id => CatalogContent.Question(id, difficulty: "moderate")), testSets);
    }

    /// <summary>A fixed-length practice set over two new questions.</summary>
    private async Task<Guid> ImportFixedSetAsync()
    {
        var ids = CatalogContent.NewIds(2);
        var setId = Guid.NewGuid();
        await _content.ImportOkAsync(FileWithQuestions(ids, CatalogContent.Set(setId, "Fixed", ids)));
        return setId;
    }

    private async Task<Guid> ImportAdaptiveSetAsync(double passingTheta = 0)
    {
        var setId = Guid.NewGuid();
        await _content.ImportOkAsync(FileWithQuestions(CatalogContent.NewIds(8), AdaptiveSet(setId, passingTheta)));
        return setId;
    }

    private static Dictionary<string, object?> AdaptiveSet(Guid id, double passingTheta) => new()
    {
        ["id"] = id,
        ["title"] = "Adaptive exam",
        ["mode"] = "cat",
        ["revealMode"] = "exam",
        ["timed"] = true,
        ["timeBudgetSec"] = BudgetSec,
        ["allowBackNav"] = false,
        ["questionIds"] = Array.Empty<Guid>(),
        ["catConfig"] = new { minItems = 3, maxItems = 6, passingTheta },
    };

    private async Task<(string User, AttemptState Attempt)> OpenedAsync(double passingTheta = 0)
    {
        var setId = await ImportAdaptiveSetAsync(passingTheta);
        var user = await Attempts.VerifiedUserAsync();
        var attempt = await StartAsync(user.AccessToken, setId, timed: true);
        await Attempts.OpenOkAsync(user.AccessToken, attempt.Id, "c-1");
        return (user.AccessToken, attempt);
    }

    private async Task<AttemptState> StartAsync(string token, Guid setId, bool timed)
    {
        var response = await StartRawAsync(token, setId, timed);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private Task<HttpResponseMessage> StartRawAsync(string token, Guid setId, bool timed) =>
        Attempts.StartAsync(token, setId, timed);

    private static object Answer(bool correct) => new { response = new { selected = correct ? "b" : "a" }, clientSeq = 1 };

    private Task<HttpResponseMessage> PostAnswerAsync(string token, Guid attemptId, int position, object body) =>
        Attempts.AnswerAsync(token, attemptId, position, "c-1", body);

    private async Task<AnswerAdaptiveItemResult> AnswerAsync(string token, Guid attemptId, int position, bool correct)
    {
        var response = await PostAnswerAsync(token, attemptId, position, Answer(correct));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AnswerAdaptiveItemResult>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task<(int Answered, AnswerAdaptiveItemResult Last)> AnswerUntilEndAsync(string token, Guid attemptId, bool correct)
    {
        var answered = 0;
        AnswerAdaptiveItemResult last;
        do
        {
            answered++;
            last = await AnswerAsync(token, attemptId, answered, correct);
        }
        while (!last.Ended && answered < 20);

        return (answered, last);
    }

    private async Task<JsonElement> ResultAsync(string token, Guid attemptId) =>
        await AttemptsApi.JsonOkAsync(await Attempts.ResultAsync(token, attemptId));
}
