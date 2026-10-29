using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Admin.Questions.Search;
using Nclex.Application.Admin.TestSets;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Admin;

public sealed class QuestionWorkflowTests(ApiFactory factory)
{
    private readonly AdminApi _admin = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));
    private readonly AuditProbe _audit = new(factory);

    [Fact]
    public async Task An_author_writes_a_question_a_reviewer_publishes_it_and_it_appears_in_a_new_practice_set()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var admin = await _admin.StaffAsync(UserRole.Admin);

        var draft = await _admin.CreateDraftAsync(author, "A brand new question about fall risk?");
        draft.Versions.ShouldHaveSingleItem().Status.ShouldBe(VersionStatus.Draft);
        draft.CurrentVersionId.ShouldBeNull();
        var versionId = draft.Versions[0].Id;

        var submitted = await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/submit-for-review", author);
        submitted.Versions[0].Status.ShouldBe(VersionStatus.InReview);
        var queue = await SearchAsync(reviewer, $"status=in_review&q={draft.Id}");
        queue.Items.ShouldHaveSingleItem().LatestStatus.ShouldBe(VersionStatus.InReview);

        var approved = await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/approve", reviewer, new { notes = "Accurate" });
        approved.Versions[0].ShouldSatisfyAllConditions(
            v => v.Status.ShouldBe(VersionStatus.Approved),
            v => v.ReviewerId.ShouldBe(reviewer.Id),
            v => v.ReviewNotes.ShouldBe("Accurate"));

        var published = await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/publish", reviewer);
        published.CurrentVersionId.ShouldBe(versionId);
        published.Versions[0].Status.ShouldBe(VersionStatus.Published);

        var set = await CreateSetAsync(admin, [draft.Id]);
        var candidate = await _attempts.VerifiedUserAsync();
        var attempt = await _attempts.StartOkAsync(candidate.AccessToken, set.Id);
        var item = await _attempts.GetItemOkAsync(candidate.AccessToken, attempt.Id, 1);
        item.GetProperty("stem").GetString().ShouldBe("A brand new question about fall risk?");
        // Before the reveal, the candidate gets neither the key nor the rationale.
        item.TryGetProperty("answerKey", out _).ShouldBeFalse();
        item.TryGetProperty("rationale", out _).ShouldBeFalse();

        var authorActions = await _audit.ActionsForAsync(author.Id);
        authorActions.ShouldContain(AuditActions.QuestionCreated);
        authorActions.ShouldContain(AuditActions.VersionSubmitted);
        (await _audit.ActionsForAsync(reviewer.Id)).ShouldContain(AuditActions.VersionApproved);
        (await _audit.ActionsForAsync(reviewer.Id)).ShouldContain(AuditActions.VersionPublished);
    }

    [Fact]
    public async Task A_reviewer_cannot_approve_their_own_version()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var question = await _admin.CreateDraftAsync(admin);
        var versionId = question.Versions[0].Id;
        await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/submit-for-review", admin);

        var response = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/approve", admin);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
        var after = await _admin.QuestionOkAsync(HttpMethod.Get, $"/api/v1/admin/questions/{question.Id}", admin);
        after.Versions[0].Status.ShouldBe(VersionStatus.InReview);
    }

    [Fact]
    public async Task Actions_out_of_order_are_refused_with_invalid_status_transition()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var question = await _admin.CreateDraftAsync(author);
        var versionId = question.Versions[0].Id;

        var approveDraft = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/approve", reviewer);
        await approveDraft.ShouldBeProblemAsync(HttpStatusCode.Conflict, "INVALID_STATUS_TRANSITION");
        var publishDraft = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/publish", reviewer);
        await publishDraft.ShouldBeProblemAsync(HttpStatusCode.Conflict, "INVALID_STATUS_TRANSITION");

        await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/submit-for-review", author);
        var editInReview = await _admin.SendAsync(HttpMethod.Put, $"/api/v1/admin/question-versions/{versionId}", author, await _admin.McDraftAsync("Changed"));
        await editInReview.ShouldBeProblemAsync(HttpStatusCode.Conflict, "INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task Publishing_a_new_version_supersedes_the_old_one_and_past_attempts_keep_theirs()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var question = await _admin.PublishedQuestionAsync(author, reviewer, "Original stem?");
        var v1 = question.CurrentVersionId!.Value;
        var set = await CreateSetAsync(admin, [question.Id]);
        var candidate = await _attempts.VerifiedUserAsync();
        var oldAttempt = await _attempts.StartOkAsync(candidate.AccessToken, set.Id);

        var withDraft = await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/questions/{question.Id}/versions", author);
        var v2 = withDraft.Versions[0];
        v2.Version.ShouldBe(2);
        v2.Status.ShouldBe(VersionStatus.Draft);
        v2.Stem.ShouldBe("Original stem?");
        await _admin.QuestionOkAsync(HttpMethod.Put, $"/api/v1/admin/question-versions/{v2.Id}", author, await _admin.McDraftAsync("Edited stem?"));
        await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{v2.Id}/submit-for-review", author);
        await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{v2.Id}/approve", reviewer);
        var republished = await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{v2.Id}/publish", reviewer);

        republished.CurrentVersionId.ShouldBe(v2.Id);
        republished.Versions.Single(v => v.Id == v1).Status.ShouldBe(VersionStatus.Superseded);
        (await AttemptVersionIdsAsync(oldAttempt.Id)).ShouldBe([v1]);
        (await _attempts.GetItemOkAsync(candidate.AccessToken, oldAttempt.Id, 1)).GetProperty("stem").GetString().ShouldBe("Original stem?");

        var otherCandidate = await _attempts.VerifiedUserAsync();
        var newAttempt = await _attempts.StartOkAsync(otherCandidate.AccessToken, set.Id);
        (await AttemptVersionIdsAsync(newAttempt.Id)).ShouldBe([v2.Id]);
    }

    [Fact]
    public async Task Search_finds_stem_text_ignoring_case_and_treats_wildcards_as_text()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var marker = Guid.NewGuid().ToString("N")[..12];
        var question = await _admin.CreateDraftAsync(author, $"Which 50% dose of Drug-{marker} is safe?");

        var found = await SearchAsync(author, $"q=DRUG-{marker.ToUpperInvariant()}");
        found.Items.ShouldHaveSingleItem().Id.ShouldBe(question.Id);

        // "%" is the literal character here, not LIKE's "anything".
        (await SearchAsync(author, $"q={Uri.EscapeDataString($"50% dose of drug-{marker}")}")).Items.ShouldHaveSingleItem();
        (await SearchAsync(author, $"q={Uri.EscapeDataString($"5% dose of drug-{marker}")}")).Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Only_one_version_can_be_worked_on_at_a_time()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var question = await _admin.CreateDraftAsync(author);

        var response = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/questions/{question.Id}/versions", author);

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task A_rejection_needs_notes_and_the_author_reworks_it_in_a_new_version()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var question = await _admin.CreateDraftAsync(author, "First try?");
        var versionId = question.Versions[0].Id;
        await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/submit-for-review", author);

        var withoutNotes = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/reject", reviewer, new { notes = " " });
        (await withoutNotes.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("notes", "REQUIRED"));

        var rejected = await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/reject", reviewer, new { notes = "Option c is also defensible." });
        rejected.Versions[0].Status.ShouldBe(VersionStatus.Rejected);
        rejected.Versions[0].ReviewNotes.ShouldBe("Option c is also defensible.");

        var reworked = await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/questions/{question.Id}/versions", author);
        reworked.Versions[0].ShouldSatisfyAllConditions(
            v => v.Version.ShouldBe(2),
            v => v.Status.ShouldBe(VersionStatus.Draft),
            v => v.Stem.ShouldBe("First try?"));
    }

    [Fact]
    public async Task A_draft_is_checked_against_its_item_type_on_every_save()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var draft = await _admin.McDraftAsync();
        draft["rationale"] = new { correct = "Why b", perOption = new { a = "A", b = "B", c = "C" } };
        draft["answerKey"] = new { correct = "z" };
        draft["stem"] = "";

        var response = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/questions", author, new { track = "RN", draft });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        var fields = problem.FieldErrors().Select(e => e.Field).ToList();
        fields.ShouldContain("stem");
        fields.ShouldContain(f => f.StartsWith("rationale.perOption", StringComparison.Ordinal));
        fields.ShouldContain(f => f.StartsWith("answerKey", StringComparison.Ordinal));
        // Each problem carries a sentence for the author.
        problem.GetProperty("errors").EnumerateArray().ShouldAllBe(e => e.GetProperty("message").GetString()!.Length > 0);
    }

    [Fact]
    public async Task A_client_needs_category_of_the_other_track_is_refused()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var draft = await _admin.McDraftAsync();

        var response = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/questions", author, new { track = "PN", draft });

        (await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("clientNeedsId", "INVALID_VALUE"));
    }

    [Fact]
    public async Task Test_the_key_grades_a_sample_answer_without_storing_it()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var question = await _admin.CreateDraftAsync(author);
        var versionId = question.Versions[0].Id;

        var right = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/preview-grade", author, new { response = new { selected = "b" } });
        right.StatusCode.ShouldBe(HttpStatusCode.OK);
        var grade = await right.Content.ReadFromJsonAsync<GradeBody>(AuthApi.Json, TestContext.Current.CancellationToken);
        grade!.Points.ShouldBe(1);
        grade.IsCorrect.ShouldBeTrue();

        var unknownOption = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/preview-grade", author, new { response = new { selected = "z" } });
        await unknownOption.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task A_retired_question_leaves_future_sets_and_cannot_be_retired_twice()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var kept = await _admin.PublishedQuestionAsync(author, reviewer, "Kept?");
        var retired = await _admin.PublishedQuestionAsync(author, reviewer, "Retired?");
        var set = await CreateSetAsync(admin, [kept.Id, retired.Id]);

        var after = await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/questions/{retired.Id}/retire", reviewer);
        after.Status.ShouldBe(QuestionStatus.Retired);
        var again = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/questions/{retired.Id}/retire", reviewer);
        await again.ShouldBeProblemAsync(HttpStatusCode.Conflict, "INVALID_STATUS_TRANSITION");

        var candidate = await _attempts.VerifiedUserAsync();
        var attempt = await _attempts.StartOkAsync(candidate.AccessToken, set.Id);
        attempt.TotalItems.ShouldBe(1);
        var addRetired = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/test-sets/{set.Id}", admin, new { questionIds = new[] { kept.Id, retired.Id } });
        (await addRetired.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("questionIds[1]", "UNKNOWN_ID"));
    }

    private async Task<QuestionSearchPage> SearchAsync(AdminApi.Staff who, string query)
    {
        var response = await _admin.SendAsync(HttpMethod.Get, $"/api/v1/admin/questions?{query}", who);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<QuestionSearchPage>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task<AdminTestSetView> CreateSetAsync(AdminApi.Staff admin, Guid[] questionIds)
    {
        var response = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/test-sets", admin, new
        {
            track = "RN",
            isPublished = true,
            questionIds,
            settings = new
            {
                title = $"Set {Guid.NewGuid():N}",
                mode = "practice",
                revealMode = "tutor",
                timed = false,
                pausePolicy = new { },
                allowBackNav = true,
                isSample = false,
                accessTier = "free",
            },
        });
        response.StatusCode.ShouldBe(HttpStatusCode.Created, await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        return (await response.Content.ReadFromJsonAsync<AdminTestSetView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private async Task<List<Guid>> AttemptVersionIdsAsync(Guid attemptId)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().AttemptItems
            .Where(i => i.AttemptId == attemptId)
            .OrderBy(i => i.Position)
            .Select(i => i.QuestionVersionId)
            .ToListAsync(TestContext.Current.CancellationToken);
    }

    private sealed record GradeBody(decimal Points, decimal MaxPoints, bool IsCorrect);
}
