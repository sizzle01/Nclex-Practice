using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Attempts;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Calls the attempt endpoints as one signed-in candidate, sending <c>X-Attempt-Client</c> on
/// writes the way the player does.
/// </summary>
public sealed class AttemptsApi(ApiFactory factory, AuthApi api)
{
    /// <summary>Signs up a candidate and marks their email verified, so they can start any free set.</summary>
    public async Task<AuthApi.SignedIn> VerifiedUserAsync()
    {
        var user = await api.SignupNewUserAsync();
        await using var scope = factory.Services.CreateAsyncScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
            .Where(u => u.Id == user.Body.User.Id)
            .ExecuteUpdateAsync(u => u.SetProperty(x => x.EmailVerifiedAt, DateTimeOffset.UtcNow), TestContext.Current.CancellationToken);
        return user;
    }

    public Task<HttpResponseMessage> StartAsync(string token, Guid testSetId) =>
        SendAsync(HttpMethod.Post, "/api/v1/attempts", token, clientId: null, new { testSetId });

    /// <summary>Starts an attempt and fails the test unless it succeeds.</summary>
    public async Task<AttemptState> StartOkAsync(string token, Guid testSetId)
    {
        var response = await StartAsync(token, testSetId);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    public Task<HttpResponseMessage> OpenAsync(string token, Guid attemptId, string? clientId) =>
        SendAsync(HttpMethod.Post, $"/api/v1/attempts/{attemptId}/open", token, clientId);

    /// <summary>Opens an attempt and fails the test unless it succeeds.</summary>
    public async Task<AttemptState> OpenOkAsync(string token, Guid attemptId, string clientId)
    {
        var response = await OpenAsync(token, attemptId, clientId);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AttemptState>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    public Task<HttpResponseMessage> GetAsync(string token, Guid attemptId) =>
        SendAsync(HttpMethod.Get, $"/api/v1/attempts/{attemptId}", token, clientId: null);

    public Task<HttpResponseMessage> GetItemAsync(string token, Guid attemptId, int position) =>
        SendAsync(HttpMethod.Get, $"/api/v1/attempts/{attemptId}/items/{position}", token, clientId: null);

    /// <summary>The item JSON exactly as sent, for checks on what a candidate can see.</summary>
    public async Task<JsonElement> GetItemOkAsync(string token, Guid attemptId, int position)
    {
        var response = await GetItemAsync(token, attemptId, position);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
    }

    public Task<HttpResponseMessage> SaveAsync(string token, Guid attemptId, int position, string? clientId, object? response, int clientSeq) =>
        SendAsync(HttpMethod.Put, $"/api/v1/attempts/{attemptId}/items/{position}/response", token, clientId, new { response, clientSeq });

    public Task<HttpResponseMessage> MarkAsync(string token, Guid attemptId, int position, string? clientId, object body) =>
        SendAsync(HttpMethod.Patch, $"/api/v1/attempts/{attemptId}/items/{position}", token, clientId, body);

    public Task<HttpResponseMessage> NavigateAsync(string token, Guid attemptId, string? clientId, int toPosition) =>
        SendAsync(HttpMethod.Post, $"/api/v1/attempts/{attemptId}/navigate", token, clientId, new { toPosition });

    public Task<HttpResponseMessage> DiscardAsync(string token, Guid attemptId) =>
        SendAsync(HttpMethod.Delete, $"/api/v1/attempts/{attemptId}", token, clientId: null);

    public Task<HttpResponseMessage> ListAsync(string token, string query = "") =>
        SendAsync(HttpMethod.Get, $"/api/v1/attempts{query}", token, clientId: null);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, string token, string? clientId, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (clientId is not null)
        {
            request.Headers.Add(AttemptClient.HeaderName, clientId);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await api.Client.SendAsync(request, TestContext.Current.CancellationToken);
    }
}
