using System.Net;
using System.Net.Http.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Admin.Subtopics;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Recommendations;

/// <summary>The subtopic list and question tags in the admin console (docs/features/F17).</summary>
public sealed class AdminSubtopicsTests(ApiFactory factory)
{
    private const string Subtopics = "/api/v1/admin/subtopics";

    private readonly AdminApi _admin = new(factory);

    private static string Unique => $"Topic {Guid.NewGuid():N}";

    [Fact]
    public async Task A_reviewer_adds_a_subtopic_and_an_author_can_read_it_but_not_add_one()
    {
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var author = await _admin.StaffAsync(UserRole.Author);
        var (clientNeedsId, _) = await _admin.TagIdsAsync();
        var name = Unique;

        var created = await CreateOkAsync(reviewer, clientNeedsId, name);
        var refused = await _admin.SendAsync(HttpMethod.Post, Subtopics, author, new { clientNeedsId, name = Unique });
        var list = await _admin.SendAsync(HttpMethod.Get, $"{Subtopics}?clientNeedsId={clientNeedsId}", author);

        (created.Track, created.ClientNeedsCode, created.Slug, created.IsActive).ShouldBe((ExamTrack.RN, "SECE.MOC", name.ToLowerInvariant().Replace(' ', '-'), true));
        await refused.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
        (await list.Content.ReadFromJsonAsync<SubtopicList>(AuthApi.Json, TestContext.Current.CancellationToken))!.Items.ShouldContain(s => s.Id == created.Id);
    }

    [Fact]
    public async Task A_candidate_can_not_read_the_list()
    {
        var candidate = await _admin.StaffAsync(UserRole.Candidate);

        var response = await _admin.SendAsync(HttpMethod.Get, Subtopics, candidate);

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
    }

    [Fact]
    public async Task A_name_with_the_same_meaning_is_refused_even_in_another_category()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var (clientNeedsId, _) = await _admin.TagIdsAsync();
        var name = Unique;
        await CreateOkAsync(admin, clientNeedsId, $"{name} tube");

        var response = await _admin.SendAsync(HttpMethod.Post, Subtopics, admin, new { clientNeedsId = clientNeedsId + 1, name = $"{name.ToUpperInvariant()} TUBES" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "SUBTOPIC_DUPLICATE");
    }

    [Fact]
    public async Task A_subtopic_must_sit_on_a_leaf()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var parent = await new CatalogContent(factory).CategoryIdAsync(ExamTrack.RN, "SECE");

        var response = await _admin.SendAsync(HttpMethod.Post, Subtopics, admin, new { clientNeedsId = parent, name = Unique });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task Renaming_keeps_the_slug_and_deactivating_hides_it_from_new_tags()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var (clientNeedsId, _) = await _admin.TagIdsAsync();
        var created = await CreateOkAsync(admin, clientNeedsId, Unique);
        var renamed = Unique;

        var patch = await _admin.SendAsync(HttpMethod.Patch, $"{Subtopics}/{created.Id}", admin, new { name = renamed, isActive = false });
        var updated = await patch.Content.ReadFromJsonAsync<SubtopicView>(AuthApi.Json, TestContext.Current.CancellationToken);
        var question = await _admin.CreateDraftAsync(admin);
        var tag = await _admin.SendAsync(HttpMethod.Put, $"/api/v1/admin/questions/{question.Id}/subtopics", admin, new { subtopicIds = new[] { created.Id } });

        (updated!.Name, updated.Slug, updated.IsActive).ShouldBe((renamed, created.Slug, false));
        await tag.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task Tagging_a_published_question_makes_no_new_version()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var published = await _admin.PublishedQuestionAsync(author, reviewer);
        var replacement = await _admin.NewSubtopicIdAsync();

        var tagged = await _admin.QuestionOkAsync(HttpMethod.Put, $"/api/v1/admin/questions/{published.Id}/subtopics", author, new { subtopicIds = new[] { replacement } });

        tagged.Versions.ShouldHaveSingleItem();
        tagged.Subtopics.ShouldHaveSingleItem().Id.ShouldBe(replacement);
    }

    [Fact]
    public async Task A_subtopic_of_another_category_can_not_be_used()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var (clientNeedsId, _) = await _admin.TagIdsAsync();
        var other = await CreateOkAsync(admin, (short)(clientNeedsId + 1), Unique);
        var question = await _admin.CreateDraftAsync(admin);

        var response = await _admin.SendAsync(HttpMethod.Put, $"/api/v1/admin/questions/{question.Id}/subtopics", admin, new { subtopicIds = new[] { other.Id } });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(4)]
    public async Task A_question_takes_one_to_three_tags(int count)
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var question = await _admin.CreateDraftAsync(admin);
        var ids = new List<Guid>();
        for (var i = 0; i < count; i++)
        {
            ids.Add(await _admin.NewSubtopicIdAsync());
        }

        var response = await _admin.SendAsync(HttpMethod.Put, $"/api/v1/admin/questions/{question.Id}/subtopics", admin, new { subtopicIds = ids });

        await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task A_retired_question_can_not_be_tagged()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var published = await _admin.PublishedQuestionAsync(author, reviewer);
        await _admin.QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/questions/{published.Id}/retire", reviewer);

        var response = await _admin.SendAsync(HttpMethod.Put, $"/api/v1/admin/questions/{published.Id}/subtopics", author, new { subtopicIds = new[] { await _admin.NewSubtopicIdAsync() } });

        await response.ShouldBeProblemAsync(HttpStatusCode.Conflict, "INVALID_STATUS_TRANSITION");
    }

    [Fact]
    public async Task A_question_without_tags_can_not_go_to_review()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var draft = await _admin.McDraftAsync();
        var question = await _admin.QuestionOkAsync(HttpMethod.Post, "/api/v1/admin/questions", author, new { track = "RN", draft });

        var response = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{question.Versions[0].Id}/submit-for-review", author);

        (await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("subtopics", "REQUIRED"));
    }

    [Fact]
    public async Task A_draft_moved_to_another_category_needs_new_tags_before_review()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var question = await _admin.CreateDraftAsync(author);
        var versionId = question.Versions[0].Id;
        var sic = await new CatalogContent(factory).CategoryIdAsync(ExamTrack.RN, "SECE.SIC");
        var draft = await _admin.McDraftAsync();
        draft["clientNeedsId"] = sic;
        await _admin.QuestionOkAsync(HttpMethod.Put, $"/api/v1/admin/question-versions/{versionId}", author, draft);

        var response = await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/submit-for-review", author);

        (await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("subtopics", "INVALID_VALUE"));
    }

    private async Task<SubtopicView> CreateOkAsync(AdminApi.Staff who, short clientNeedsId, string name)
    {
        var response = await _admin.SendAsync(HttpMethod.Post, Subtopics, who, new { clientNeedsId, name });
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<SubtopicView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }
}
