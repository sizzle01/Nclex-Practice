using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Flags;

/// <summary>Reporting a question and listing my reports (api.md section 7).</summary>
public sealed class ReportIssueTests(ApiFactory factory)
{
    private readonly AuthApi _auth = new(factory);
    private readonly FlagsApi _flags = new(factory, new AuthApi(factory));
    private readonly FlagScenario _scenario = new(factory);

    [Fact]
    public async Task A_candidate_reports_a_question_from_an_attempt_and_sees_it_in_my_reports()
    {
        var candidate = await _scenario.CandidateInAttemptAsync();
        var version = candidate.VersionIds[0];

        var created = await _flags.ReportOkAsync(candidate.Token, version, new
        {
            reason = "incorrect_answer_key",
            comment = "  I think C is also right.  ",
            attemptId = candidate.AttemptId,
        });

        created.GetProperty("questionVersionId").GetGuid().ShouldBe(version);
        created.GetProperty("reason").GetString().ShouldBe("incorrect_answer_key");
        created.GetProperty("status").GetString().ShouldBe("open");
        created.GetProperty("comment").GetString().ShouldBe("I think C is also right.");
        created.GetProperty("attemptId").GetGuid().ShouldBe(candidate.AttemptId);
        created.GetProperty("stemPreview").GetString().ShouldBe("Which action should the nurse take first?");
        var mine = await FlagsApi.JsonAsync(await _flags.MineAsync(candidate.Token));
        FlagsApi.Ids(mine).ShouldBe([created.GetProperty("id").GetGuid()]);
    }

    [Fact]
    public async Task No_flag_response_carries_the_answer_key_or_rationale()
    {
        var candidate = await _scenario.CandidateInAttemptAsync();
        await _flags.ReportOkAsync(candidate.Token, candidate.VersionIds[0], new { reason = "typo" });

        var body = await (await _flags.MineAsync(candidate.Token)).Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // CatalogContent's key is { correct: "b" } and its rationale says "Why b".
        body.ShouldNotContain("Why b");
        body.ShouldNotContain("\"correct\"");
        body.ShouldNotContain("rationale\":");
    }

    [Fact]
    public async Task The_same_reason_twice_is_a_duplicate_but_another_reason_is_a_new_report()
    {
        var candidate = await _scenario.CandidateInAttemptAsync();
        var version = candidate.VersionIds[1];
        await _flags.ReportOkAsync(candidate.Token, version, new { reason = "typo" });

        await (await _flags.ReportAsync(candidate.Token, version, new { reason = "typo", comment = "again" }))
            .ShouldBeProblemAsync(HttpStatusCode.Conflict, "FLAG_DUPLICATE");
        await _flags.ReportOkAsync(candidate.Token, version, new { reason = "media_problem" });
    }

    [Fact]
    public async Task Two_users_can_report_the_same_version_for_the_same_reason()
    {
        var first = await _scenario.CandidateInAttemptAsync();
        var second = await _auth.SignupNewUserAsync();

        await _flags.ReportOkAsync(first.Token, first.VersionIds[0], new { reason = "typo" });
        await _flags.ReportOkAsync(second.AccessToken, first.VersionIds[0], new { reason = "typo" });
    }

    [Fact]
    public async Task An_unknown_version_is_not_found()
    {
        var user = await _auth.SignupNewUserAsync();

        await (await _flags.ReportAsync(user.AccessToken, Guid.NewGuid(), new { reason = "typo" }))
            .ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task A_version_candidates_never_see_is_not_found()
    {
        var candidate = await _scenario.CandidateInAttemptAsync();
        var version = candidate.VersionIds[2];
        await SetVersionStatusAsync(version, VersionStatus.Draft);

        await (await _flags.ReportAsync(candidate.Token, version, new { reason = "typo" }))
            .ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task A_superseded_version_can_still_be_reported_from_an_old_attempt()
    {
        var candidate = await _scenario.CandidateInAttemptAsync();
        var version = candidate.VersionIds[2];
        await SetVersionStatusAsync(version, VersionStatus.Superseded);

        await _flags.ReportOkAsync(candidate.Token, version, new { reason = "outdated_guideline", attemptId = candidate.AttemptId });
    }

    [Fact]
    public async Task Someone_elses_attempt_is_not_found()
    {
        var owner = await _scenario.CandidateInAttemptAsync();
        var other = await _scenario.CandidateInAttemptAsync();

        await (await _flags.ReportAsync(other.Token, owner.VersionIds[0], new { reason = "typo", attemptId = owner.AttemptId }))
            .ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    [Fact]
    public async Task An_attempt_that_did_not_serve_the_version_is_rejected()
    {
        var mine = await _scenario.CandidateInAttemptAsync();
        var elsewhere = await _scenario.CandidateInAttemptAsync();

        var problem = await (await _flags.ReportAsync(mine.Token, elsewhere.VersionIds[0], new { reason = "typo", attemptId = mine.AttemptId }))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("attemptId", "INVALID_VALUE")]);
    }

    [Fact]
    public async Task A_missing_reason_or_a_long_comment_is_rejected()
    {
        var candidate = await _scenario.CandidateInAttemptAsync();

        var missing = await (await _flags.ReportAsync(candidate.Token, candidate.VersionIds[0], new { comment = "hi" }))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        missing.FieldErrors().ShouldContain(("reason", "REQUIRED"));

        var tooLong = await (await _flags.ReportAsync(candidate.Token, candidate.VersionIds[0], new { reason = "other", comment = new string('x', 2001) }))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        tooLong.FieldErrors().ShouldBe([("comment", "TOO_LONG")]);
    }

    [Fact]
    public async Task An_unknown_reason_is_rejected()
    {
        var candidate = await _scenario.CandidateInAttemptAsync();

        await (await _flags.ReportAsync(candidate.Token, candidate.VersionIds[0], new { reason = "boring" }))
            .ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task Reporting_needs_a_sign_in()
    {
        var response = await _auth.SendWithTokenAsync(HttpMethod.Post, $"/api/v1/question-versions/{Guid.NewGuid()}/flags", null, new { reason = "typo" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task My_reports_page_newest_first_and_show_only_mine()
    {
        var candidate = await _scenario.CandidateInAttemptAsync();
        var ids = new List<Guid>();
        foreach (var version in candidate.VersionIds)
        {
            ids.Add((await _flags.ReportOkAsync(candidate.Token, version, new { reason = "typo" })).GetProperty("id").GetGuid());
        }

        var other = await _auth.SignupNewUserAsync();

        var first = await FlagsApi.JsonAsync(await _flags.MineAsync(candidate.Token, "?limit=2"));
        var second = await FlagsApi.JsonAsync(await _flags.MineAsync(candidate.Token, $"?limit=2&cursor={first.GetProperty("nextCursor").GetString()}"));
        FlagsApi.Ids(first).ShouldBe([ids[2], ids[1]]);
        FlagsApi.Ids(second).ShouldBe([ids[0]]);
        FlagsApi.Ids(await FlagsApi.JsonAsync(await _flags.MineAsync(other.AccessToken))).ShouldBeEmpty();
    }

    private async Task SetVersionStatusAsync(Guid versionId, VersionStatus status)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().QuestionVersions
            .Where(v => v.Id == versionId)
            .ExecuteUpdateAsync(v => v.SetProperty(x => x.Status, status), TestContext.Current.CancellationToken);
    }
}
