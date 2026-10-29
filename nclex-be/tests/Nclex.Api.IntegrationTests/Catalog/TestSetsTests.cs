using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.ListTestSets;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Catalog;

public sealed class TestSetsTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);
    private readonly CatalogContent _content = new(factory);

    [Fact]
    public async Task Sets_are_listed_sample_first_then_practice_then_challenge_with_their_card_fields()
    {
        var (questions, ids) = (CatalogContent.NewIds(10), CatalogContent.NewIds(3));
        var prefix = $"Order {Guid.NewGuid():N}";
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets:
        [
            CatalogContent.Set(ids[0], $"{prefix} A challenge", questions, new
            {
                mode = "challenge", revealMode = "exam", timed = true, timeBudgetSec = 4800, passThreshold = 0.65m,
            }),
            CatalogContent.Set(ids[1], $"{prefix} B practice", questions.Take(4)),
            CatalogContent.Set(ids[2], $"{prefix} C sample", questions, new { isSample = true }),
        ]));
        var user = await _api.SignupNewUserAsync();

        var all = await ListAllAsync(user.AccessToken, "/api/v1/test-sets?kind=prebuilt");

        var mine = all.Where(s => ids.Contains(s.Id)).ToList();
        mine.Select(s => s.Id).ShouldBe([ids[2], ids[1], ids[0]]);
        mine[0].ShouldBe(new TestSetSummary(
            ids[2], $"{prefix} C sample", TestMode.Practice, RevealMode.Tutor, 10, false, null, null, null,
            IsSample: true, PlanTier.Free, Locked: false, MyLastAttempt: null));
        mine[1].QuestionCount.ShouldBe(4);
        mine[2].ShouldBe(new TestSetSummary(
            ids[0], $"{prefix} A challenge", TestMode.Challenge, RevealMode.Exam, 10, true, 4800, null, 0.65m,
            IsSample: false, PlanTier.Free, Locked: false, MyLastAttempt: null));
    }

    [Fact]
    public async Task Set_json_matches_the_api_contract_and_holds_no_items()
    {
        var (questions, setId) = (CatalogContent.NewIds(1), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets: [CatalogContent.Set(setId, "Contract", questions)]));
        var user = await _api.SignupNewUserAsync();

        var list = await GetJsonAsync(user.AccessToken, $"/api/v1/test-sets?kind=prebuilt&limit=50{await CursorToAsync(user.AccessToken, setId)}");
        var detail = await GetJsonAsync(user.AccessToken, $"/api/v1/test-sets/{setId}");

        list.TryGetProperty("nextCursor", out _).ShouldBeTrue();
        var item = list.GetProperty("items").EnumerateArray().Single(i => i.GetProperty("id").GetGuid() == setId);
        item.GetProperty("mode").GetString().ShouldBe("practice");
        item.GetProperty("accessTier").GetString().ShouldBe("free");
        item.GetProperty("myLastAttempt").ValueKind.ShouldBe(JsonValueKind.Null);
        detail.GetProperty("revealMode").GetString().ShouldBe("tutor");
        detail.GetProperty("kind").GetString().ShouldBe("prebuilt");
        detail.GetProperty("track").GetString().ShouldBe("RN");
        detail.GetProperty("pausePolicy").GetRawText().ShouldBe("""{"maxPauses":null,"maxPauseSec":null}""");
        detail.GetProperty("myLastAttempt").ValueKind.ShouldBe(JsonValueKind.Null);

        // Nothing about the questions themselves, let alone keys or rationales.
        var raw = detail.GetRawText() + list.GetRawText();
        raw.ShouldNotContain(questions[0].ToString());
        raw.ShouldNotContain("answerKey", Case.Insensitive);
        raw.ShouldNotContain("rationale", Case.Insensitive);
    }

    [Fact]
    public async Task Premium_sets_are_locked_for_free_users_and_open_for_premium_users()
    {
        var (questions, setId) = (CatalogContent.NewIds(1), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets:
            [CatalogContent.Set(setId, "Premium", questions, new { accessTier = "premium" })]));
        var free = await _api.SignupNewUserAsync();
        var premium = await _api.SignupNewUserAsync();
        await _content.MakePremiumAsync(premium.Body.User.Id);

        var forFree = (await ListAllAsync(free.AccessToken, "/api/v1/test-sets")).Single(s => s.Id == setId);
        var forPremium = (await ListAllAsync(premium.AccessToken, "/api/v1/test-sets")).Single(s => s.Id == setId);
        var detailForFree = await GetDetailAsync(free.AccessToken, setId);

        (forFree.AccessTier, forFree.Locked).ShouldBe((PlanTier.Premium, true));
        (forPremium.AccessTier, forPremium.Locked).ShouldBe((PlanTier.Premium, false));
        // The detail is still shown, so the panel can say why the set is locked.
        detailForFree.Locked.ShouldBeTrue();
    }

    [Fact]
    public async Task Sets_of_the_other_track_are_only_listed_when_asked_for()
    {
        var (questions, setId) = (CatalogContent.NewIds(1), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("PN", questions, testSets: [CatalogContent.Set(setId, "PN set", questions)]));
        var rnUser = await _api.SignupNewUserAsync();

        var byDefault = await ListAllAsync(rnUser.AccessToken, "/api/v1/test-sets");
        var rn = await ListAllAsync(rnUser.AccessToken, "/api/v1/test-sets?track=RN");
        var pn = await ListAllAsync(rnUser.AccessToken, "/api/v1/test-sets?track=PN");

        byDefault.ShouldNotContain(s => s.Id == setId);
        rn.ShouldNotContain(s => s.Id == setId);
        pn.ShouldContain(s => s.Id == setId);
    }

    [Fact]
    public async Task Unpublished_sets_are_neither_listed_nor_shown()
    {
        var (questions, setId) = (CatalogContent.NewIds(1), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets: [CatalogContent.Set(setId, "Hidden", questions)]));
        await _content.UnpublishSetAsync(setId);
        var user = await _api.SignupNewUserAsync();

        var list = await ListAllAsync(user.AccessToken, "/api/v1/test-sets");
        var detail = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{setId}", user.AccessToken);

        list.ShouldNotContain(s => s.Id == setId);
        await detail.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task Pages_follow_the_cursor_without_repeating_sets()
    {
        var questions = CatalogContent.NewIds(1);
        var ids = CatalogContent.NewIds(3);
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets:
            [.. ids.Select((id, i) => CatalogContent.Set(id, $"Paged {i}", questions))]));
        var user = await _api.SignupNewUserAsync();

        // Other test classes import sets in parallel; hold them off while the pages are walked.
        var (first, all) = await CatalogContent.WhileCatalogIsStillAsync(async () => (
            await GetPageAsync(user.AccessToken, "/api/v1/test-sets?limit=1"),
            await ListAllAsync(user.AccessToken, "/api/v1/test-sets", limit: 2)));

        first.Items.Count.ShouldBe(1);
        first.NextCursor.ShouldNotBeNull();
        all.Select(s => s.Id).ShouldBeUnique();
        ids.ShouldAllBe(id => all.Any(s => s.Id == id));
    }

    [Fact]
    public async Task A_set_detail_has_the_settings_for_the_panel()
    {
        var (questions, setId) = (CatalogContent.NewIds(2), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", questions, testSets:
        [
            CatalogContent.Set(setId, "Detailed", questions, new
            {
                description = "Two questions.", timed = true, perQuestionSec = 90, allowBackNav = false,
                pausePolicy = new { maxPauses = 2, maxPauseSec = 300 },
            }),
        ]));
        var user = await _api.SignupNewUserAsync();

        var detail = await GetDetailAsync(user.AccessToken, setId);

        detail.ShouldBe(new TestSetDetail(
            setId, ExamTrack.RN, TestSetKind.Prebuilt, "Detailed", "Two questions.", TestMode.Practice, RevealMode.Tutor,
            2, true, null, 90, null, new PausePolicy(2, 300), AllowBackNav: false, IsSample: false, PlanTier.Free,
            Locked: false, MyLastAttempt: null));
    }

    [Fact]
    public async Task An_unknown_set_returns_404()
    {
        var user = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{Guid.NewGuid()}", user.AccessToken);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Theory]
    [InlineData("kind=custom", "kind")]
    [InlineData("limit=0", "limit")]
    [InlineData("cursor=nonsense", "cursor")]
    public async Task Bad_query_values_return_400(string query, string field)
    {
        var user = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets?{query}", user.AccessToken);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([(field, "INVALID_VALUE")]);
    }

    [Theory]
    [InlineData("/api/v1/test-sets")]
    [InlineData("/api/v1/test-sets/0192f0a0-0000-7000-8000-000000000001")]
    public async Task Test_sets_require_sign_in(string path)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, path, accessToken: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    // Other tests add sets to the same database, so tests read every page and pick out their own.
    private async Task<List<TestSetSummary>> ListAllAsync(string accessToken, string path, int limit = 50)
    {
        var separator = path.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        var sets = new List<TestSetSummary>();
        string? cursor = null;
        do
        {
            var page = await GetPageAsync(accessToken, $"{path}{separator}limit={limit}{(cursor is null ? "" : $"&cursor={cursor}")}");
            sets.AddRange(page.Items);
            cursor = page.NextCursor;
        }
        while (cursor is not null);

        return sets;
    }

    // The cursor of the page holding the set, as a query-string suffix ("" for the first page).
    private async Task<string> CursorToAsync(string accessToken, Guid setId)
    {
        string? cursor = null;
        while (true)
        {
            var page = await GetPageAsync(accessToken, $"/api/v1/test-sets?limit=50{(cursor is null ? "" : $"&cursor={cursor}")}");
            if (page.Items.Any(s => s.Id == setId))
            {
                return cursor is null ? "" : $"&cursor={cursor}";
            }

            cursor = page.NextCursor ?? throw new InvalidOperationException($"Set {setId} is not listed.");
        }
    }

    private async Task<TestSetPage> GetPageAsync(string accessToken, string path)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, path, accessToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<TestSetPage>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task<TestSetDetail> GetDetailAsync(string accessToken, Guid id)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{id}", accessToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<TestSetDetail>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task<JsonElement> GetJsonAsync(string accessToken, string path)
    {
        var response = await _api.SendWithTokenAsync(HttpMethod.Get, path, accessToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
    }
}
