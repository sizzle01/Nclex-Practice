using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Application.WeakAreas.GetWeakAreas;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.WeakAreas;

/// <summary>
/// "Practice your weak areas" through the real API (docs/features/F18). Each test uses a fresh user
/// and its own questions. Other tests add questions to the same Client Needs leaves, so a test
/// checks which questions must or must not be in the practice set, never the exact pool size.
/// </summary>
public sealed class WeakAreasTests(ApiFactory factory)
{
    private const string GetPath = "/api/v1/me/weak-areas";
    private const string StartPath = "/api/v1/test-sets/weak-areas";
    private const string Device = "c-1";

    private readonly AuthApi _api = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));
    private readonly CatalogContent _content = new(factory);

    [Fact]
    public async Task A_new_user_has_no_weak_areas_and_can_not_start_a_test()
    {
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;

        var view = await GetOkAsync(user);
        var start = await StartAsync(user);

        (view.Areas.Count, view.AttemptsConsidered, view.QuestionCount).ShouldBe((0, 0, 0));
        await start.ShouldBeProblemAsync(HttpStatusCode.UnprocessableEntity, "NO_WEAK_AREAS");
    }

    [Fact]
    public async Task A_weak_area_is_listed_and_strong_areas_are_not()
    {
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var weak = await ImportSetAsync("SECE.MOC", 5);
        var strong = await ImportSetAsync("PSI", 5);
        await SubmitAsync(user, weak.SetId, weak.Questions.Count, "a"); // all wrong
        await SubmitAsync(user, strong.SetId, strong.Questions.Count, "b"); // all right

        var view = await GetOkAsync(user);

        var area = view.Areas.ShouldHaveSingleItem();
        (area.Code, area.Pct, area.Items).ShouldBe(("SECE.MOC", 0m, 5));
        view.AttemptsConsidered.ShouldBe(2);
        view.QuestionsAvailable.ShouldBeGreaterThanOrEqualTo(5);
        view.QuestionCount.ShouldBe(Math.Min(25, view.QuestionsAvailable));
    }

    [Fact]
    public async Task The_test_is_a_tutor_practice_set_in_the_weak_area_with_every_missed_question()
    {
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var weak = await ImportSetAsync("SECE.MOC", 5);
        await SubmitAsync(user, weak.SetId, weak.Questions.Count, "a");
        var leaf = await _content.CategoryIdAsync(ExamTrack.RN, "SECE.MOC");

        var response = await StartAsync(user);

        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var created = (await response.Content.ReadFromJsonAsync<CreateCustomSetResponse>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        created.Attempt.ShouldNotBeNull();
        created.TestSet.ShouldSatisfyAllConditions(
            s => s.Kind.ShouldBe(TestSetKind.Custom),
            s => s.Title.ShouldBe($"Weak areas practice: {s.QuestionCount} questions"),
            s => s.QuestionCount.ShouldBeInRange(5, 25),
            s => s.Mode.ShouldBe(TestMode.Practice),
            s => s.RevealMode.ShouldBe(RevealMode.Tutor),
            s => s.Timed.ShouldBeFalse());

        var served = await ServedQuestionsAsync(created.TestSet.Id);
        served.Count.ShouldBe(created.TestSet.QuestionCount);
        served.ShouldAllBe(q => q.ClientNeedsId == leaf && q.CaseStudyId == null);
        // The missed questions come first, and 5 always fit in the test.
        weak.Questions.ShouldAllBe(id => served.Any(q => q.QuestionId == id));
    }

    [Fact]
    public async Task Only_the_last_ten_attempts_count()
    {
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var weak = await ImportSetAsync("SECE.MOC", 5);
        var strong = await ImportSetAsync("PSI", 1);
        await SubmitAsync(user, weak.SetId, weak.Questions.Count, "a");
        for (var i = 0; i < 10; i++)
        {
            await SubmitAsync(user, strong.SetId, 1, "b");
        }

        var view = await GetOkAsync(user);

        view.Areas.ShouldBeEmpty();
        view.AttemptsConsidered.ShouldBe(10);
    }

    [Fact]
    public async Task A_free_user_never_gets_a_premium_question()
    {
        var signedIn = await _attempts.VerifiedUserAsync();
        var weak = await ImportSetAsync("SECE.MOC", 5);
        await SubmitAsync(signedIn.AccessToken, weak.SetId, weak.Questions.Count, "a");
        var premium = Guid.NewGuid();
        await _content.ImportOkAsync(CatalogContent.FileWith("RN", [CatalogContent.Question(premium, "SECE.MOC", accessTier: "premium")]));
        // A missed question would be picked first, so leaving it out proves the plan filter.
        await _content.AddHistoryAsync(signedIn.Body.User.Id, false, premium);

        var response = await StartAsync(signedIn.AccessToken);

        var created = (await response.Content.ReadFromJsonAsync<CreateCustomSetResponse>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        (await ServedQuestionsAsync(created.TestSet.Id)).ShouldNotContain(q => q.QuestionId == premium);
    }

    [Fact]
    public async Task An_unknown_track_is_refused()
    {
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"{GetPath}?track=XX", user);

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task Signed_out_callers_are_refused()
    {
        var response = await factory.CreateClient().GetAsync(GetPath, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private async Task<WeakAreasView> GetOkAsync(string token)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, GetPath, token);
        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return (await response.Content.ReadFromJsonAsync<WeakAreasView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private Task<HttpResponseMessage> StartAsync(string token) =>
        _api.SendWithTokenAsync(HttpMethod.Post, StartPath, token, new { });

    // A set of multiple-choice questions (key b) in one leaf, in exam reveal so a whole attempt submits at once.
    private async Task<ImportedSet> ImportSetAsync(string clientNeeds, int count)
    {
        var questions = CatalogContent.NewIds(count);
        var setId = Guid.NewGuid();
        await _content.ImportOkAsync(CatalogContent.FileWith(
            "RN",
            [.. questions.Select(q => CatalogContent.Question(q, clientNeeds))],
            testSets: [CatalogContent.Set(setId, $"Weak areas {setId:N}", questions, new { revealMode = "exam" })]));
        return new ImportedSet(setId, questions);
    }

    private async Task SubmitAsync(string token, Guid setId, int items, string answer)
    {
        var attempt = await _attempts.StartOkAsync(token, setId);
        await _attempts.OpenOkAsync(token, attempt.Id, Device);
        for (var position = 1; position <= items; position++)
        {
            await _attempts.SaveAsync(token, attempt.Id, position, Device, new { selected = answer }, position);
        }

        await AttemptsApi.JsonOkAsync(await _attempts.SubmitAsync(token, attempt.Id, Device));
    }

    private async Task<List<ServedQuestion>> ServedQuestionsAsync(Guid setId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await (
            from item in db.TestSetItems
            where item.TestSetId == setId
            join question in db.Questions on item.QuestionId equals question.Id
            join version in db.QuestionVersions on question.CurrentVersionId equals version.Id
            select new ServedQuestion(item.QuestionId, version.ClientNeedsId, version.CaseStudyId))
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    private sealed record ImportedSet(Guid SetId, List<Guid> Questions);

    private sealed record ServedQuestion(Guid QuestionId, short ClientNeedsId, Guid? CaseStudyId);
}
