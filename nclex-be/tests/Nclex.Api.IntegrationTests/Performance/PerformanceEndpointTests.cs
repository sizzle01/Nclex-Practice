using System.Net;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Performance;

/// <summary>GET /me/performance/*, history scores, myLastAttempt and the admin rebuild (api.md sections 3, 4, 5, 8).</summary>
public sealed class PerformanceEndpointTests(ApiFactory factory) : IAsyncLifetime
{
    private readonly TimedTestHost _host = new(factory);
    private PerformanceScenario _scenario = null!;

    public async ValueTask InitializeAsync()
    {
        _scenario = new PerformanceScenario(factory, _host);
        await _scenario.ImportSetAsync();
    }

    public ValueTask DisposeAsync() => _host.DisposeAsync();

    // Summary and breakdown

    [Fact]
    public async Task The_summary_is_all_zero_before_the_first_attempt()
    {
        var candidate = await _scenario.CandidateAsync();

        var summary = await GetOkAsync(candidate, "/api/v1/me/performance/summary");

        summary.GetProperty("track").GetString().ShouldBe("RN");
        (Int(summary, "attemptsCount"), Int(summary, "itemsAnswered"), summary.GetProperty("accuracy").GetDecimal())
            .ShouldBe((0, 0, 0m));
        summary.GetProperty("weakest").GetArrayLength().ShouldBe(0);
        summary.TryGetProperty("streak", out _).ShouldBeFalse();
    }

    [Fact]
    public async Task The_summary_shows_the_totals_and_the_weakest_categories()
    {
        var candidate = await _scenario.CandidateAsync();
        await SubmitAndRefreshAsync(candidate, PerformanceScenario.AllRight);
        await SubmitAndRefreshAsync(candidate, PerformanceScenario.AllWrong);
        // First item right, the others wrong: SECE.MOC 2 of 3, SECE.SIC and PSI 1 of 3 (by points).
        await SubmitAndRefreshAsync(candidate, PerformanceScenario.AllWrong with { First = "b" });

        var summary = await GetOkAsync(candidate, "/api/v1/me/performance/summary?track=RN");

        (Int(summary, "attemptsCount"), Int(summary, "itemsAnswered"), Int(summary, "itemsCorrect"), summary.GetProperty("accuracy").GetDecimal())
            .ShouldBe((3, 9, 4, 0.4000m));
        var weakest = summary.GetProperty("weakest").EnumerateArray().ToList();
        weakest.Select(w => w.GetProperty("key").GetString()).ShouldBe(["PSI", "SECE.SIC", "SECE.MOC"]);
        weakest[0].GetProperty("name").GetString().ShouldNotBe("PSI");
        (weakest[0].GetProperty("accuracy").GetDecimal(), Int(weakest[0], "itemsAnswered")).ShouldBe((0.3333m, 3));
    }

    [Fact]
    public async Task The_breakdown_lists_every_category_of_the_dimension_with_its_name()
    {
        var candidate = await _scenario.CandidateAsync();
        await SubmitAndRefreshAsync(candidate, PerformanceScenario.AllRight);
        await SubmitAndRefreshAsync(candidate, PerformanceScenario.AllWrong);

        var breakdown = await GetOkAsync(candidate, "/api/v1/me/performance/breakdown?dimension=item_type");

        breakdown.GetProperty("dimension").GetString().ShouldBe("item_type");
        var rows = breakdown.GetProperty("items").EnumerateArray()
            .Select(r => $"{r.GetProperty("key").GetString()} {Int(r, "itemsAnswered")} {r.GetProperty("accuracy").GetDecimal():0.0000} x{Int(r, "attemptsCount")}")
            .ToList();
        rows.ShouldBe(["multiple_choice 4 0.5000 x2", "sata 2 0.5000 x2"], ignoreOrder: true);
    }

    [Theory]
    [InlineData("/api/v1/me/performance/breakdown", "dimension")]
    [InlineData("/api/v1/me/performance/breakdown?dimension=overall", "dimension")]
    [InlineData("/api/v1/me/performance/trend?dimension=client_needs", "key")]
    [InlineData("/api/v1/me/performance/trend?from=2026-09-14&to=2026-09-01", "to")]
    public async Task Invalid_queries_are_refused(string path, string field)
    {
        var candidate = await _scenario.CandidateAsync();

        var response = await _host.Auth.SendWithTokenAsync(HttpMethod.Get, path, candidate.Token);

        (await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED"))
            .FieldErrors().Select(e => e.Field).ShouldContain(field);
    }

    [Fact]
    public async Task Performance_needs_a_signed_in_user()
    {
        var response = await _host.Auth.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me/performance/summary", accessToken: null);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    // Trend

    [Fact]
    public async Task Trend_weeks_follow_the_users_time_zone()
    {
        var candidate = await _scenario.CandidateAsync();
        var sunday = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        var monday = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllWrong);
        // Sunday 13 September 23:30 and Monday 14 September 13:00 in Los Angeles: both Monday in UTC.
        await _scenario.SetSubmittedAtAsync(sunday, "2026-09-14T06:30:00Z");
        await _scenario.SetSubmittedAtAsync(monday, "2026-09-14T20:00:00Z");

        var inUtc = await TrendAsync(candidate, "bucket=week");
        await _scenario.SetTimezoneAsync(candidate, "America/Los_Angeles");
        var inLosAngeles = await TrendAsync(candidate, "bucket=week");

        inUtc.ShouldBe(["2026-09-14 6 0.5000"]);
        inLosAngeles.ShouldBe(["2026-09-07 3 1.0000", "2026-09-14 3 0.0000"]);
    }

    [Fact]
    public async Task Trend_dates_are_local_days_and_include_the_whole_last_day()
    {
        var candidate = await _scenario.CandidateAsync();
        await _scenario.SetTimezoneAsync(candidate, "America/Los_Angeles");
        var sunday = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        var monday = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllWrong);
        await _scenario.SetSubmittedAtAsync(sunday, "2026-09-14T06:30:00Z");
        await _scenario.SetSubmittedAtAsync(monday, "2026-09-15T06:59:00Z"); // Monday 23:59 PDT

        (await TrendAsync(candidate, "bucket=day&from=2026-09-14&to=2026-09-14")).ShouldBe(["2026-09-14 3 0.0000"]);
        (await TrendAsync(candidate, "bucket=day&to=2026-09-13")).ShouldBe(["2026-09-13 3 1.0000"]);
    }

    [Fact]
    public async Task A_category_trend_follows_that_category_only()
    {
        var candidate = await _scenario.CandidateAsync();
        var right = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        var wrong = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllWrong);
        await _scenario.SetSubmittedAtAsync(right, "2026-09-01T12:00:00Z");
        await _scenario.SetSubmittedAtAsync(wrong, "2026-09-02T12:00:00Z");

        var trend = await GetOkAsync(candidate, "/api/v1/me/performance/trend?dimension=client_needs&key=SECE.SIC&bucket=day");

        (trend.GetProperty("dimension").GetString(), trend.GetProperty("key").GetString(), trend.GetProperty("bucket").GetString())
            .ShouldBe(("client_needs", "SECE.SIC", "day"));
        Series(trend).ShouldBe(["2026-09-01 1 1.0000", "2026-09-02 1 0.0000"]);
    }

    // History and catalog

    [Fact]
    public async Task History_rows_carry_the_score_and_drafts_do_not()
    {
        var candidate = await _scenario.CandidateAsync();
        var submitted = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        var draft = await _host.Attempts.StartOkAsync(candidate.Token, _scenario.SetId);

        var history = await AttemptsApi.JsonOkAsync(await _host.Attempts.ListAsync(candidate.Token, "?status=submitted"));
        var drafts = await AttemptsApi.JsonOkAsync(await _host.Attempts.ListAsync(candidate.Token));

        var row = history.GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        (row.GetProperty("id").GetGuid(), row.GetProperty("scorePct").GetDecimal()).ShouldBe((submitted, 1.0000m));
        row.GetProperty("passed").ValueKind.ShouldBe(JsonValueKind.Null);
        var draftRow = drafts.GetProperty("items").EnumerateArray().ShouldHaveSingleItem();
        (draftRow.GetProperty("id").GetGuid(), draftRow.GetProperty("scorePct").ValueKind).ShouldBe((draft.Id, JsonValueKind.Null));
    }

    [Fact]
    public async Task A_test_set_shows_my_latest_submitted_attempt()
    {
        var candidate = await _scenario.CandidateAsync();
        var before = await GetOkAsync(candidate, $"/api/v1/test-sets/{_scenario.SetId}");
        await _scenario.SubmitAsync(candidate, PerformanceScenario.AllRight);
        var latest = await _scenario.SubmitAsync(candidate, PerformanceScenario.AllWrong);

        var after = await GetOkAsync(candidate, $"/api/v1/test-sets/{_scenario.SetId}");

        before.GetProperty("myLastAttempt").ValueKind.ShouldBe(JsonValueKind.Null);
        var last = after.GetProperty("myLastAttempt");
        (last.GetProperty("id").GetGuid(), last.GetProperty("scorePct").GetDecimal()).ShouldBe((latest, 0m));
    }

    // Admin rebuild

    [Fact]
    public async Task An_admin_can_queue_a_rebuild_of_a_users_snapshots()
    {
        var candidate = await _scenario.CandidateAsync();
        var admin = await _scenario.AdminTokenAsync(await _scenario.CandidateAsync());

        var response = await _host.Auth.SendWithTokenAsync(HttpMethod.Post, "/api/v1/admin/performance-snapshots/rebuild", admin, new { userId = candidate.Id });

        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        _host.Jobs.SnapshotRebuilds.ShouldBe([candidate.Id]);
    }

    [Fact]
    public async Task A_candidate_cannot_queue_a_rebuild()
    {
        var candidate = await _scenario.CandidateAsync();

        var response = await _host.Auth.SendWithTokenAsync(HttpMethod.Post, "/api/v1/admin/performance-snapshots/rebuild", candidate.Token, new { userId = candidate.Id });

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
        _host.Jobs.SnapshotRebuilds.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_rebuild_of_an_unknown_user_is_not_found()
    {
        var admin = await _scenario.AdminTokenAsync(await _scenario.CandidateAsync());

        var response = await _host.Auth.SendWithTokenAsync(HttpMethod.Post, "/api/v1/admin/performance-snapshots/rebuild", admin, new { userId = Guid.NewGuid() });

        await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, "NOT_FOUND");
    }

    private async Task SubmitAndRefreshAsync(PerformanceScenario.Candidate candidate, PerformanceScenario.Answers answers) =>
        await _scenario.RefreshAsync(candidate, await _scenario.SubmitAsync(candidate, answers));

    private async Task<JsonElement> GetOkAsync(PerformanceScenario.Candidate candidate, string path) =>
        await AttemptsApi.JsonOkAsync(await _host.Auth.SendWithTokenAsync(HttpMethod.Get, path, candidate.Token));

    private async Task<List<string>> TrendAsync(PerformanceScenario.Candidate candidate, string query) =>
        Series(await GetOkAsync(candidate, $"/api/v1/me/performance/trend?{query}"));

    private static List<string> Series(JsonElement trend) =>
        [.. trend.GetProperty("series").EnumerateArray().Select(p =>
            $"{p.GetProperty("bucketStart").GetString()} {Int(p, "items")} {p.GetProperty("pct").GetDecimal():0.0000}")];

    private static int Int(JsonElement element, string property) => element.GetProperty(property).GetInt32();
}
