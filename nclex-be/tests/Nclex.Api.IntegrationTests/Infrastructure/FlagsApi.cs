using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Calls the content-feedback endpoints (api.md sections 7 and 8) and makes moderator accounts.</summary>
public sealed class FlagsApi(ApiFactory factory, AuthApi api)
{
    public Task<HttpResponseMessage> ReportAsync(string token, Guid versionId, object body) =>
        api.SendWithTokenAsync(HttpMethod.Post, $"/api/v1/question-versions/{versionId}/flags", token, body);

    /// <summary>Reports a version and fails the test unless it was created.</summary>
    public async Task<JsonElement> ReportOkAsync(string token, Guid versionId, object body)
    {
        var response = await ReportAsync(token, versionId, body);
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        return await JsonAsync(response);
    }

    public Task<HttpResponseMessage> MineAsync(string token, string query = "") =>
        api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/me/flags{query}", token);

    public Task<HttpResponseMessage> QueueAsync(string token, string query = "") =>
        api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/admin/flags{query}", token);

    public Task<HttpResponseMessage> UpdateAsync(string token, Guid flagId, object body) =>
        api.SendWithTokenAsync(HttpMethod.Patch, $"/api/v1/admin/flags/{flagId}", token, body);

    public async Task<JsonElement> UpdateOkAsync(string token, Guid flagId, object body)
    {
        var response = await UpdateAsync(token, flagId, body);
        response.EnsureSuccessStatusCode();
        return await JsonAsync(response);
    }

    /// <summary>
    /// A new account with <paramref name="role"/>, signed in again so the access token carries it.
    /// There is no role-changing endpoint until the admin users screen (api.md section 8).
    /// </summary>
    public async Task<AuthApi.SignedIn> UserWithRoleAsync(UserRole role)
    {
        var user = await api.SignupNewUserAsync();
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
                .Where(u => u.Id == user.Body.User.Id)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.Role, role), TestContext.Current.CancellationToken);
        }

        return await api.LoginOkAsync(user.Body.User.Email);
    }

    public static async Task<JsonElement> JsonAsync(HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);

    /// <summary>The ids of a page's items, in order.</summary>
    public static List<Guid> Ids(JsonElement page) =>
        [.. page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid())];
}
