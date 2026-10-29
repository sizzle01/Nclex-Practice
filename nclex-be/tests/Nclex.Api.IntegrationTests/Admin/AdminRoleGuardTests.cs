using System.Net;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Admin;

/// <summary>The role column of api.md section 8: A = author, R = reviewer, Ad = admin.</summary>
public sealed class AdminRoleGuardTests(ApiFactory factory)
{
    private readonly AdminApi _admin = new(factory);

    [Fact]
    public async Task Signed_out_callers_get_401()
    {
        var response = await _admin.SendAsync(HttpMethod.Get, "/api/v1/admin/questions", who: null);

        await response.ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Theory]
    [InlineData("GET", "/api/v1/admin/questions")]
    [InlineData("POST", "/api/v1/admin/media/upload-url")]
    [InlineData("GET", "/api/v1/admin/test-sets")]
    [InlineData("GET", "/api/v1/admin/question-banks")]
    [InlineData("GET", "/api/v1/admin/users")]
    public async Task Candidates_can_use_none_of_the_console(string method, string path)
    {
        var candidate = await _admin.StaffAsync(UserRole.Candidate);

        var response = await _admin.SendAsync(new HttpMethod(method), path, candidate, method == "GET" ? null : new { });

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
    }

    [Fact]
    public async Task Authors_cannot_review_publish_retire_or_manage_sets()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var question = await _admin.CreateDraftAsync(author);
        var versionId = question.Versions[0].Id;

        foreach (var path in new[]
        {
            $"/api/v1/admin/question-versions/{versionId}/approve",
            $"/api/v1/admin/question-versions/{versionId}/reject",
            $"/api/v1/admin/question-versions/{versionId}/publish",
            $"/api/v1/admin/questions/{question.Id}/retire",
        })
        {
            var response = await _admin.SendAsync(HttpMethod.Post, path, author, new { notes = "x" });
            await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
        }

        var sets = await _admin.SendAsync(HttpMethod.Get, "/api/v1/admin/test-sets", author);
        await sets.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
    }

    [Fact]
    public async Task Reviewers_read_and_test_questions_but_do_not_write_them()
    {
        var author = await _admin.StaffAsync(UserRole.Author);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var question = await _admin.CreateDraftAsync(author);
        var versionId = question.Versions[0].Id;

        (await _admin.SendAsync(HttpMethod.Get, $"/api/v1/admin/questions/{question.Id}", reviewer)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await _admin.SendAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/preview-grade", reviewer, new { response = new { selected = "a" } }))
            .StatusCode.ShouldBe(HttpStatusCode.OK);

        var create = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/questions", reviewer, new { track = "RN", draft = await _admin.McDraftAsync() });
        await create.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
        var edit = await _admin.SendAsync(HttpMethod.Put, $"/api/v1/admin/question-versions/{versionId}", reviewer, await _admin.McDraftAsync());
        await edit.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
        var users = await _admin.SendAsync(HttpMethod.Get, "/api/v1/admin/users", reviewer);
        await users.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
    }
}
