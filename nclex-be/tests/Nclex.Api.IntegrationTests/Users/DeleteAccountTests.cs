using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Users;

/// <summary>DELETE /me: personal data is removed, the row is kept anonymised (database.md section 4).</summary>
public sealed class DeleteAccountTests(ApiFactory factory)
{
    private const string Path = "/api/v1/me";

    private readonly AuthApi _api = new(factory);
    private readonly AuditProbe _audit = new(factory);

    [Fact]
    public async Task Deleting_scrubs_personal_data_but_keeps_the_row()
    {
        var email = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync(email);
        var id = user.Body.User.Id;

        var response = await DeleteAsync(user.AccessToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var stored = await LoadUserAsync(id);
        stored.Email.ShouldBe($"deleted+{id}@invalid");
        stored.PasswordHash.ShouldBeNull();
        stored.DisplayName.ShouldBeNull();
        stored.DeletedAt.ShouldNotBeNull();
        // Kept, so attempts that point at this row stay usable, anonymised, for item statistics.
        stored.Track.ShouldBe(ExamTrack.RN);
    }

    [Fact]
    public async Task Deleting_removes_sessions_link_tokens_and_emails()
    {
        var email = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync(email);
        var id = user.Body.User.Id;
        await _api.LoginOkAsync(email);

        await DeleteAsync(user.AccessToken);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ct = TestContext.Current.CancellationToken;
        (await db.RefreshSessions.CountAsync(s => s.UserId == id, ct)).ShouldBe(0);
        (await db.EmailTokens.CountAsync(t => t.UserId == id, ct)).ShouldBe(0);
        (await db.EmailOutbox.CountAsync(m => m.UserId == id, ct)).ShouldBe(0);
    }

    [Fact]
    public async Task Audit_events_are_kept_without_ip_addresses_and_the_deletion_is_recorded()
    {
        var email = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync(email);
        await _api.LoginOkAsync(email);
        await _api.LoginAsync(email, "wrong-password-123");

        await DeleteAsync(user.AccessToken);

        var events = await _audit.ForUserAsync(user.Body.User.Id);
        events.Select(e => e.Action).ShouldBe(["auth.login_succeeded", "auth.login_failed", "account.deleted"]);
        events.ShouldAllBe(e => e.Ip == null && e.Metadata == "{}");
    }

    [Fact]
    public async Task Existing_access_and_refresh_tokens_stop_working()
    {
        var user = await _api.SignupNewUserAsync();

        var response = await DeleteAsync(user.AccessToken);

        AuthApi.RefreshSetCookie(response).ShouldNotBeNull().ShouldContain("expires=Thu, 01 Jan 1970");
        await (await _api.SendWithTokenAsync(HttpMethod.Get, Path, user.AccessToken)).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
        await (await _api.RefreshAsync(user.RefreshToken)).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "UNAUTHENTICATED");
    }

    [Fact]
    public async Task The_old_address_can_no_longer_log_in_but_can_sign_up_again()
    {
        var email = AuthApi.NewEmail();
        var user = await _api.SignupNewUserAsync(email);

        await DeleteAsync(user.AccessToken);

        await (await _api.LoginAsync(email, AuthApi.Password)).ShouldBeProblemAsync(HttpStatusCode.Unauthorized, "INVALID_CREDENTIALS");
        var again = await _api.SignupNewUserAsync(email);
        again.Body.User.Id.ShouldNotBe(user.Body.User.Id);
    }

    [Fact]
    public async Task Deleting_needs_the_current_password()
    {
        var user = await _api.SignupNewUserAsync();

        var response = await _api.SendWithTokenAsync(HttpMethod.Delete, Path, user.AccessToken, new { currentPassword = "" });

        var problem = await response.ShouldBeProblemAsync(HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        problem.FieldErrors().ShouldBe([("currentPassword", "REQUIRED")]);
    }

    private Task<HttpResponseMessage> DeleteAsync(string accessToken) =>
        _api.SendWithTokenAsync(HttpMethod.Delete, Path, accessToken, new { currentPassword = AuthApi.Password });

    private async Task<User> LoadUserAsync(Guid id)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
            .SingleAsync(u => u.Id == id, TestContext.Current.CancellationToken);
    }
}
