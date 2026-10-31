using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Recommendations;

/// <summary>GET /attempts/{id}/recommendations through the real API (docs/features/F17).</summary>
public sealed class RecommendationsTests(ApiFactory factory)
{
    private const string Device = "c-1";

    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));
    private readonly AdminApi _admin = new(factory);

    private static object Mc(string option) => new { selected = option };

    [Fact]
    public async Task Missed_subtopics_are_ranked_and_correct_or_unanswered_ones_are_left_out()
    {
        var test = await ImportTestAsync();
        await _attempts.SaveAsync(test.User, test.AttemptId, 1, Device, Mc("a"), 1); // wrong, shared
        await _attempts.SaveAsync(test.User, test.AttemptId, 2, Device, Mc("b"), 2); // right, shared
        await _attempts.SaveAsync(test.User, test.AttemptId, 3, Device, new { selected = new[] { "a", "b" } }, 3); // partial credit, partial
        await _attempts.SaveAsync(test.User, test.AttemptId, 4, Device, Mc("b"), 4); // right, correct
        // Item 5 stays unanswered.
        await SubmitAsync(test);

        var body = await GetOkAsync(test);

        Subtopics(body).ShouldBe([($"Partial {test.Tag}", 1, 1), ($"Shared {test.Tag}", 1, 2)]);
    }

    [Fact]
    public async Task The_response_holds_no_answer_keys_or_rationales()
    {
        var test = await ImportTestAsync();
        await _attempts.SaveAsync(test.User, test.AttemptId, 1, Device, Mc("a"), 1);
        await SubmitAsync(test);

        var body = await GetOkAsync(test);

        var raw = body.GetRawText();
        raw.ShouldNotContain("Why", Case.Sensitive);
        raw.ShouldNotContain("Because", Case.Sensitive);
        raw.ShouldNotContain("answerKey", Case.Insensitive);
        raw.ShouldNotContain("rationale", Case.Insensitive);
        raw.ShouldNotContain("stem", Case.Insensitive);
    }

    [Fact]
    public async Task A_perfect_test_suggests_nothing()
    {
        var test = await ImportTestAsync();
        await _attempts.SaveAsync(test.User, test.AttemptId, 1, Device, Mc("b"), 1);
        await SubmitAsync(test);

        Subtopics(await GetOkAsync(test)).ShouldBeEmpty();
    }

    [Fact]
    public async Task A_deactivated_subtopic_is_no_longer_suggested_on_past_results()
    {
        var test = await ImportTestAsync();
        await _attempts.SaveAsync(test.User, test.AttemptId, 1, Device, Mc("a"), 1);
        await SubmitAsync(test);

        await using (var scope = factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Subtopics
                .Where(s => s.Name == $"Shared {test.Tag}")
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false), TestContext.Current.CancellationToken);
        }

        Subtopics(await GetOkAsync(test)).ShouldBeEmpty();
    }

    [Fact]
    public async Task Correcting_a_tag_changes_the_suggestions_for_a_past_result()
    {
        var test = await ImportTestAsync();
        await _attempts.SaveAsync(test.User, test.AttemptId, 1, Device, Mc("a"), 1);
        await SubmitAsync(test);
        var author = await _admin.StaffAsync(UserRole.Author);
        var replacement = await _admin.NewSubtopicIdAsync();

        await _admin.QuestionOkAsync(HttpMethod.Put, $"/api/v1/admin/questions/{test.Questions[0]}/subtopics", author, new { subtopicIds = new[] { replacement } });

        var name = (await GetOkAsync(test)).GetProperty("subtopics").EnumerateArray().Single().GetProperty("id").GetGuid();
        name.ShouldBe(replacement);
    }

    [Fact]
    public async Task Another_candidates_attempt_is_not_found()
    {
        var test = await ImportTestAsync();
        await SubmitAsync(test);
        var other = await _attempts.VerifiedUserAsync();

        var response = await _attempts.RecommendationsAsync(other.AccessToken, test.AttemptId);

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task An_attempt_that_is_not_submitted_has_no_suggestions_yet()
    {
        var test = await ImportTestAsync();

        var response = await _attempts.RecommendationsAsync(test.User, test.AttemptId);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "ATTEMPT_NOT_ACTIVE");
    }

    private async Task SubmitAsync(TestAttempt test) =>
        (await _attempts.SubmitAsync(test.User, test.AttemptId, Device)).EnsureSuccessStatusCode();

    private async Task<JsonElement> GetOkAsync(TestAttempt test) =>
        await AttemptsApi.JsonOkAsync(await _attempts.RecommendationsAsync(test.User, test.AttemptId));

    private static List<(string, int, int)> Subtopics(JsonElement body) =>
        [.. body.GetProperty("subtopics").EnumerateArray()
            .Select(s => (s.GetProperty("name").GetString()!, s.GetProperty("missed").GetInt32(), s.GetProperty("answered").GetInt32()))];

    // Five items: two tagged "shared", then "partial", "correct" and "unanswered", all named with a
    // tag of their own because names are unique across the track.
    private async Task<TestAttempt> ImportTestAsync()
    {
        var tag = Guid.NewGuid().ToString("N");
        var (questions, setId) = (CatalogContent.NewIds(5), Guid.NewGuid());
        string Slug(string topic) => $"{topic}-{tag}";
        var content = new List<Dictionary<string, object>>
        {
            CatalogContent.Question(questions[0]),
            CatalogContent.Question(questions[1]),
            CatalogContent.SataQuestion(questions[2]),
            CatalogContent.Question(questions[3]),
            CatalogContent.Question(questions[4]),
        };
        string[] slugs = ["shared", "shared", "partial", "correct", "unanswered"];
        for (var i = 0; i < content.Count; i++)
        {
            content[i]["subtopics"] = new[] { Slug(slugs[i]) };
        }

        await _content.ImportOkAsync(QuestionImportFile.Parse(JsonSerializer.Serialize(new
        {
            track = "RN",
            subtopics = new[]
            {
                new { clientNeeds = "SECE.MOC", slug = Slug("shared"), name = $"Shared {tag}" },
                new { clientNeeds = "SECE.SIC", slug = Slug("partial"), name = $"Partial {tag}" },
                new { clientNeeds = "SECE.MOC", slug = Slug("correct"), name = $"Correct {tag}" },
                new { clientNeeds = "SECE.MOC", slug = Slug("unanswered"), name = $"Unanswered {tag}" },
            },
            questions = content,
            testSets = new[] { CatalogContent.Set(setId, $"Set {tag}", questions, new { revealMode = "exam" }) },
        })));

        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, attempt.Id, Device);
        return new TestAttempt(user, attempt.Id, questions, tag);
    }

    private sealed record TestAttempt(string User, Guid AttemptId, List<Guid> Questions, string Tag);
}
