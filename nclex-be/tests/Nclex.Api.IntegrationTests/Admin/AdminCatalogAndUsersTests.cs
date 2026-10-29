using System.Net;
using System.Net.Http.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Admin.Banks;
using Nclex.Application.Admin.TestSets;
using Nclex.Application.Admin.Users;
using Nclex.Domain.Entities;

namespace Nclex.Api.IntegrationTests.Admin;

public sealed class AdminCatalogAndUsersTests(ApiFactory factory)
{
    private readonly AdminApi _admin = new(factory);
    private readonly AuthApi _auth = new(factory);

    [Fact]
    public async Task A_set_starts_unpublished_and_is_published_later()
    {
        var (admin, questionId) = await AdminWithPublishedQuestionAsync();

        var created = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/test-sets", admin, SetBody([questionId], isPublished: false));
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var set = (await created.Content.ReadFromJsonAsync<AdminTestSetView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        set.IsPublished.ShouldBeFalse();
        set.Questions.ShouldHaveSingleItem().PublishedVersion.ShouldBe(1);

        var candidate = await _auth.SignupNewUserAsync();
        var hidden = await _auth.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{set.Id}", candidate.AccessToken);
        hidden.StatusCode.ShouldBe(HttpStatusCode.NotFound);

        var patched = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/test-sets/{set.Id}", admin, new { isPublished = true });
        patched.StatusCode.ShouldBe(HttpStatusCode.OK);
        var shown = await _auth.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/test-sets/{set.Id}", candidate.AccessToken);
        shown.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task A_set_refuses_drafts_duplicates_and_inconsistent_timers()
    {
        var (admin, questionId) = await AdminWithPublishedQuestionAsync();
        var draft = await _admin.CreateDraftAsync(admin);

        var withDraft = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/test-sets", admin, SetBody([questionId, draft.Id]));
        (await withDraft.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("questionIds[1]", "UNKNOWN_ID"));

        var twice = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/test-sets", admin, SetBody([questionId, questionId]));
        (await twice.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED")).FieldErrors().ShouldContain(("questionIds[1]", "DUPLICATE_ID"));

        var timedWithoutTimer = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/test-sets", admin, SetBody([questionId], timed: true));
        await timedWithoutTimer.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
    }

    [Fact]
    public async Task A_bank_is_created_and_its_membership_replaced()
    {
        var (admin, questionId) = await AdminWithPublishedQuestionAsync();

        var created = await _admin.SendAsync(HttpMethod.Post, "/api/v1/admin/question-banks", admin, new
        {
            track = "RN",
            name = "Safety basics",
            accessTier = "free",
            isPublished = false,
        });
        created.StatusCode.ShouldBe(HttpStatusCode.Created);
        var bank = (await created.Content.ReadFromJsonAsync<AdminBankView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        bank.Questions.ShouldBeEmpty();

        var patched = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/question-banks/{bank.Id}", admin, new { questionIds = new[] { questionId }, isPublished = true, description = "Start here" });
        var updated = (await patched.Content.ReadFromJsonAsync<AdminBankView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        updated.ShouldSatisfyAllConditions(
            b => b.Questions.ShouldHaveSingleItem().QuestionId.ShouldBe(questionId),
            b => b.IsPublished.ShouldBeTrue(),
            b => b.Description.ShouldBe("Start here"),
            b => b.Name.ShouldBe("Safety basics"));
    }

    [Fact]
    public async Task An_admin_changes_a_role_and_the_user_must_sign_in_again()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var email = AuthApi.NewEmail();
        var user = await _auth.SignupNewUserAsync(email);

        var response = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/users/{user.Body.User.Id}", admin, new { role = "author", planTier = "premium" });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var view = (await response.Content.ReadFromJsonAsync<AdminUserView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        view.Role.ShouldBe(UserRole.Author);
        view.PlanTier.ShouldBe(PlanTier.Premium);
        // Their refresh session was revoked, so the old role can't be renewed.
        (await _auth.RefreshAsync(user.RefreshToken)).StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        var again = await _auth.LoginOkAsync(email);
        again.Body.User.Role.ShouldBe(UserRole.Author);
    }

    [Fact]
    public async Task An_admin_cannot_change_their_own_role()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);

        var response = await _admin.SendAsync(HttpMethod.Patch, $"/api/v1/admin/users/{admin.Id}", admin, new { role = "candidate" });

        await response.ShouldBeProblemAsync(HttpStatusCode.Forbidden, "FORBIDDEN");
    }

    [Fact]
    public async Task Users_are_found_by_part_of_their_email()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var email = AuthApi.NewEmail();
        var user = await _auth.SignupNewUserAsync(email);

        var response = await _admin.SendAsync(HttpMethod.Get, $"/api/v1/admin/users?q={email[..20].ToUpperInvariant()}", admin);

        var page = (await response.Content.ReadFromJsonAsync<AdminUserPage>(AuthApi.Json, TestContext.Current.CancellationToken))!;
        page.Items.ShouldHaveSingleItem().Id.ShouldBe(user.Body.User.Id);
    }

    private async Task<(AdminApi.Staff Admin, Guid QuestionId)> AdminWithPublishedQuestionAsync()
    {
        var admin = await _admin.StaffAsync(UserRole.Admin);
        var reviewer = await _admin.StaffAsync(UserRole.Reviewer);
        var question = await _admin.PublishedQuestionAsync(admin, reviewer);
        return (admin, question.Id);
    }

    private static object SetBody(Guid[] questionIds, bool isPublished = true, bool timed = false) => new
    {
        track = "RN",
        isPublished,
        questionIds,
        settings = new
        {
            title = $"Set {Guid.NewGuid():N}",
            mode = "practice",
            revealMode = "tutor",
            timed,
            pausePolicy = new { },
            allowBackNav = true,
            isSample = false,
            accessTier = "free",
        },
    };
}
