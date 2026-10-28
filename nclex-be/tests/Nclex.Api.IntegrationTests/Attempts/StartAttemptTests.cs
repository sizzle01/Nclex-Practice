using System.Net;
using System.Net.Http.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Attempts;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Attempts;

public sealed class StartAttemptTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);
    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));

    [Fact]
    public async Task Starting_a_set_creates_an_untimed_attempt_at_item_1()
    {
        var (setId, _) = await ImportSetAsync(3);
        var user = await _attempts.VerifiedUserAsync();

        var response = await _attempts.StartAsync(user.AccessToken, setId);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var state = (await response.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        response.Headers.Location!.ToString().ShouldEndWith($"/api/v1/attempts/{state.Id}");
        state.ShouldSatisfyAllConditions(
            s => s.TestSet.Id.ShouldBe(setId),
            s => s.Status.ShouldBe(AttemptStatus.InProgress),
            s => s.TotalItems.ShouldBe(3),
            s => s.CurrentPosition.ShouldBe(1),
            s => s.Progress.ShouldBe(new AttemptProgress(0, 0, 0)),
            s => s.Navigator.Select(n => n.State).ShouldBe([ItemState.Unseen, ItemState.Unseen, ItemState.Unseen]),
            s => s.Timer.Exam.ShouldBeNull(),
            s => s.Timer.Item.ShouldBeNull(),
            s => s.StaleDraft.ShouldBeFalse(),
            s => s.ResultAvailable.ShouldBeFalse());
    }

    [Fact]
    public async Task Starting_the_same_prebuilt_set_again_returns_the_existing_draft()
    {
        var (setId, _) = await ImportSetAsync(2);
        var user = await _attempts.VerifiedUserAsync();
        var first = await _attempts.StartOkAsync(user.AccessToken, setId);

        var again = await _attempts.StartAsync(user.AccessToken, setId);

        again.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await again.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!.Id.ShouldBe(first.Id);
    }

    [Fact]
    public async Task After_discarding_a_draft_starting_again_creates_a_new_attempt()
    {
        var (setId, _) = await ImportSetAsync(2);
        var user = await _attempts.VerifiedUserAsync();
        var first = await _attempts.StartOkAsync(user.AccessToken, setId);
        await _attempts.DiscardAsync(user.AccessToken, first.Id);

        var again = await _attempts.StartAsync(user.AccessToken, setId);

        again.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task An_unverified_user_may_start_only_sample_sets()
    {
        var (setId, _) = await ImportSetAsync(2);
        var (sampleId, _) = await ImportSetAsync(TestSet.SampleQuestionCount, new { isSample = true });
        var user = await _api.SignupNewUserAsync();

        var normal = await _attempts.StartAsync(user.AccessToken, setId);
        var sample = await _attempts.StartAsync(user.AccessToken, sampleId);

        await normal.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "EMAIL_NOT_VERIFIED");
        sample.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Premium_sets_need_a_premium_plan()
    {
        var (setId, _) = await ImportSetAsync(2, new { accessTier = "premium" });
        var free = await _attempts.VerifiedUserAsync();
        var premium = await _attempts.VerifiedUserAsync();
        await _content.MakePremiumAsync(premium.Body.User.Id);

        var forFree = await _attempts.StartAsync(free.AccessToken, setId);
        var forPremium = await _attempts.StartAsync(premium.AccessToken, setId);

        await forFree.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "PLAN_UPGRADE_REQUIRED");
        forPremium.StatusCode.ShouldBe(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Unknown_and_unpublished_sets_are_not_found()
    {
        var (setId, _) = await ImportSetAsync(1);
        await _content.UnpublishSetAsync(setId);
        var user = await _attempts.VerifiedUserAsync();

        await (await _attempts.StartAsync(user.AccessToken, setId)).ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        await (await _attempts.StartAsync(user.AccessToken, Guid.NewGuid())).ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task Retired_questions_are_left_out_of_a_new_attempt()
    {
        var (setId, questions) = await ImportSetAsync(3);
        await _content.RetireQuestionAsync(questions[1]);
        var user = await _attempts.VerifiedUserAsync();

        var state = await _attempts.StartOkAsync(user.AccessToken, setId);

        state.TotalItems.ShouldBe(2);
    }

    [Fact]
    public async Task A_missing_test_set_id_is_a_validation_error()
    {
        var user = await _attempts.VerifiedUserAsync();

        var response = await _attempts.StartAsync(user.AccessToken, Guid.Empty);

        (await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().ShouldContain(("testSetId", "REQUIRED"));
    }

    private async Task<(Guid SetId, List<Guid> Questions)> ImportSetAsync(int questionCount, object? settings = null)
    {
        var (questions, setId) = (CatalogContent.NewIds(questionCount), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets:
            [CatalogContent.Set(setId, $"Attempts {setId:N}", questions, settings)]));
        return (setId, questions);
    }
}
