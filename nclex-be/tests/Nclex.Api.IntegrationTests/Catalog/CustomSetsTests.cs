using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Catalog;

/// <summary>
/// The builder endpoints. Each test gives a fresh user wrong answers on its own questions and
/// builds from the "incorrect" pool, so counts are exact even though other tests share the database.
/// </summary>
public sealed class CustomSetsTests(ApiFactory factory)
{
    private const string CountPath = "/api/v1/test-sets/custom/available-count";
    private const string CreatePath = "/api/v1/test-sets/custom";

    private readonly AuthApi _api = new(factory);
    private readonly CatalogContent _content = new(factory);

    [Fact]
    public async Task The_available_count_matches_the_filters()
    {
        var (user, questions) = await UserWithWrongAnswersAsync(3);

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"{CountPath}?pool=incorrect&itemTypes=multiple_choice", user.AccessToken);
        var sata = await _api.SendWithTokenAsync(HttpMethod.Get, $"{CountPath}?pool=incorrect&itemTypes=sata", user.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        (await response.Content.ReadFromJsonAsync<AvailableCountResponse>(AuthApi.Json, TestContext.Current.CancellationToken))
            .ShouldBe(new AvailableCountResponse(questions.Count));
        (await sata.Content.ReadFromJsonAsync<AvailableCountResponse>(AuthApi.Json, TestContext.Current.CancellationToken))
            .ShouldBe(new AvailableCountResponse(0));
    }

    [Fact]
    public async Task A_custom_set_is_created_for_its_owner_with_the_right_number_of_items()
    {
        var (user, questions) = await UserWithWrongAnswersAsync(7);

        var response = await CreateAsync(user.AccessToken, new
        {
            pool = "incorrect",
            questionCount = 5,
            mode = "challenge",
            revealMode = "exam",
            timing = new { timed = true, timeBudgetSec = 600 },
            startNow = true,
        });

        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        var created = (await response.Content.ReadFromJsonAsync<CreateCustomSetResponse>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        response.Headers.Location!.AbsolutePath.ShouldBe($"/api/v1/test-sets/{created.TestSet.Id}");
        created.Attempt.ShouldBeNull(); // attempts start from F08
        created.TestSet.ShouldSatisfyAllConditions(
            s => s.Kind.ShouldBe(TestSetKind.Custom),
            s => s.Title.ShouldBe("Custom test: 5 questions"),
            s => s.QuestionCount.ShouldBe(5),
            s => s.Mode.ShouldBe(TestMode.Challenge),
            s => s.TimeBudgetSec.ShouldBe(600),
            s => s.PassThreshold.ShouldBe(0.65m),
            s => s.Locked.ShouldBeFalse());

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var set = await db.TestSets.SingleAsync(s => s.Id == created.TestSet.Id, TestContext.Current.CancellationToken);
        (set.OwnerUserId, set.IsPublished).ShouldBe((user.Body.User.Id, false));
        set.GeneratorCriteria!.RootElement.GetProperty("pool").GetString().ShouldBe("incorrect");
        var items = await db.TestSetItems.Where(i => i.TestSetId == set.Id).OrderBy(i => i.Position)
            .ToListAsync(TestContext.Current.CancellationToken);
        items.Select(i => i.Position).ShouldBe([1, 2, 3, 4, 5]);
        items.Select(i => i.QuestionId).ShouldBeUnique();
        items.ShouldAllBe(i => questions.Contains(i.QuestionId));
    }

    [Fact]
    public async Task Only_the_owner_can_see_a_custom_set()
    {
        var (owner, _) = await UserWithWrongAnswersAsync(5);
        var created = await CreateOkAsync(owner.AccessToken, new { pool = "incorrect", questionCount = 5, mode = "practice", revealMode = "tutor" });
        var someoneElse = await _api.SignupNewUserAsync();

        var forOwner = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{created.TestSet.Id}", owner.AccessToken);
        var forOther = await _api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{created.TestSet.Id}", someoneElse.AccessToken);
        var prebuiltList = await _api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/test-sets?limit=50", owner.AccessToken);

        forOwner.StatusCode.ShouldBe(HttpStatusCode.OK);
        await forOther.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
        (await prebuiltList.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).ShouldNotContain(created.TestSet.Id.ToString());
    }

    [Fact]
    public async Task Asking_for_more_than_match_returns_insufficient_questions_with_the_count()
    {
        var (user, _) = await UserWithWrongAnswersAsync(3);

        var response = await CreateAsync(user.AccessToken, new { pool = "incorrect", questionCount = 5, mode = "practice", revealMode = "tutor" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.UnprocessableEntity, "INSUFFICIENT_QUESTIONS");
        problem.GetProperty("available").GetInt32().ShouldBe(3);
    }

    [Fact]
    public async Task Invalid_requests_return_field_errors()
    {
        var user = await _api.SignupNewUserAsync();

        var response = await CreateAsync(user.AccessToken, new { questionCount = 2, mode = "cat", timing = new { timed = true } });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe(
        [
            ("questionCount", "INVALID_VALUE"),
            ("mode", "INVALID_VALUE"),
            ("revealMode", "REQUIRED"),
            ("timing.timed", "INVALID_VALUE"),
        ]);
    }

    [Fact]
    public async Task An_unknown_category_is_a_field_error()
    {
        var user = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(HttpMethod.Get, $"{CountPath}?clientNeedsIds=999", user.AccessToken);

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("clientNeedsIds", "INVALID_VALUE")]);
    }

    [Theory]
    [InlineData("GET", CountPath)]
    [InlineData("POST", CreatePath)]
    public async Task The_builder_requires_sign_in(string method, string path)
    {
        var response = await _api.SendWithTokenAsync(new HttpMethod(method), path, accessToken: null, body: method == "POST" ? new { } : null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    private async Task<(AuthApi.SignedIn User, List<Guid> Questions)> UserWithWrongAnswersAsync(int count)
    {
        var questions = CatalogContent.NewIds(count);
        await _content.ImportOkAsync(CatalogContent.File("RN", questions));
        var user = await _api.SignupNewUserAsync();
        await _content.AddHistoryAsync(user.Body.User.Id, correct: false, [.. questions]);
        return (user, questions);
    }

    private Task<HttpResponseMessage> CreateAsync(string accessToken, object body) =>
        _api.SendWithTokenAsync(HttpMethod.Post, CreatePath, accessToken, body);

    private async Task<CreateCustomSetResponse> CreateOkAsync(string accessToken, object body)
    {
        var response = await CreateAsync(accessToken, body);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<CreateCustomSetResponse>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }
}
