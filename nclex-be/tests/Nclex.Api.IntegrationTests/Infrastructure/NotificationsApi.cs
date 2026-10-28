using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>Calls the notification endpoints (api.md section 6) as one signed-in user.</summary>
public sealed class NotificationsApi(AuthApi api)
{
    public Task<HttpResponseMessage> ListAsync(string token, string query = "") =>
        api.SendWithTokenAsync(HttpMethod.Get, $"/api/v1/notifications{query}", token);

    /// <summary>The feed page as JSON, failing the test unless the request succeeds.</summary>
    public async Task<JsonElement> ListOkAsync(string token, string query = "") =>
        await AttemptsApi.JsonOkAsync(await ListAsync(token, query));

    public async Task<int> UnreadCountAsync(string token)
    {
        var body = await AttemptsApi.JsonOkAsync(await api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/notifications/unread-count", token));
        return body.GetProperty("count").GetInt32();
    }

    public Task<HttpResponseMessage> MarkReadAsync(string token, Guid id) =>
        api.SendWithTokenAsync(HttpMethod.Post, $"/api/v1/notifications/{id}/read", token);

    public Task<HttpResponseMessage> MarkAllReadAsync(string token) =>
        api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/notifications/read-all", token);

    public Task<HttpResponseMessage> StreamTicketAsync(string token) =>
        api.SendWithTokenAsync(HttpMethod.Post, "/api/v1/notifications/stream-ticket", token);

    public async Task<string> StreamTicketOkAsync(string token)
    {
        var body = await AttemptsApi.JsonOkAsync(await StreamTicketAsync(token));
        return body.GetProperty("ticket").GetString()!;
    }

    /// <summary>Opens the SSE stream and returns once the headers arrive; the body keeps streaming.</summary>
    public async Task<HttpResponseMessage> OpenStreamAsync(string? ticket)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/v1/notifications/stream?ticket={Uri.EscapeDataString(ticket ?? string.Empty)}");
        return await api.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, TestContext.Current.CancellationToken);
    }

    public Task<HttpResponseMessage> PreferencesAsync(string token) =>
        api.SendWithTokenAsync(HttpMethod.Get, "/api/v1/me/notification-preferences", token);

    public Task<HttpResponseMessage> ReplacePreferencesAsync(string token, object body) =>
        api.SendWithTokenAsync(HttpMethod.Put, "/api/v1/me/notification-preferences", token, body);

    /// <summary>Every category on in-app, with email only for those named.</summary>
    public static object Preferences(bool inAppResults = true, params string[] emailFor) => new
    {
        preferences = new[] { "results", "reminders", "engagement", "content", "security" }.Select(category => new
        {
            category,
            inApp = category != "results" || inAppResults,
            email = category == "security" || emailFor.Contains(category),
        }),
    };

    public async Task ReplacePreferencesOkAsync(string token, object body) =>
        (await ReplacePreferencesAsync(token, body)).StatusCode.ShouldBe(HttpStatusCode.OK);

    /// <summary>The ids in a feed page, in order.</summary>
    public static List<Guid> Ids(JsonElement page) =>
        [.. page.GetProperty("items").EnumerateArray().Select(i => i.GetProperty("id").GetGuid())];
}
