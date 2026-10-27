using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using Nclex.Application.Auth;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Calls the auth endpoints the way the browser does, but handles the refresh cookie by hand so
/// tests can inspect it and replay old values (for reuse detection).
/// </summary>
public sealed class AuthApi(ApiFactory factory)
{
    public const string Password = "correct-horse-battery";

    /// <summary>Same JSON conventions as the API: camelCase and snake_case enums.</summary>
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) },
    };

    /// <summary>
    /// HTTPS base address, because the refresh cookie is Secure; automatic cookie handling is off
    /// so each test controls exactly which cookie it sends.
    /// </summary>
    public HttpClient Client { get; } = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = new Uri("https://localhost"),
        HandleCookies = false,
    });

    /// <summary>A unique email per call, so tests sharing one database never collide.</summary>
    public static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";

    public Task<HttpResponseMessage> SignupAsync(object body) =>
        Client.PostAsJsonAsync("/api/v1/auth/signup", body, TestContext.Current.CancellationToken);

    /// <summary>Signs up a new candidate and returns their session.</summary>
    public async Task<SignedIn> SignupNewUserAsync(string? email = null)
    {
        var response = await SignupAsync(new { email = email ?? NewEmail(), password = Password, track = "RN", displayName = "Test" });
        response.EnsureSuccessStatusCode();
        return await SignedIn.FromAsync(response);
    }

    public Task<HttpResponseMessage> LoginAsync(string email, string password) =>
        Client.PostAsJsonAsync("/api/v1/auth/login", new { email, password }, TestContext.Current.CancellationToken);

    public async Task<SignedIn> LoginOkAsync(string email)
    {
        var response = await LoginAsync(email, Password);
        response.EnsureSuccessStatusCode();
        return await SignedIn.FromAsync(response);
    }

    /// <summary>POSTs a JSON body to an endpoint that needs no sign-in.</summary>
    public Task<HttpResponseMessage> PostAsync(string path, object body) =>
        Client.PostAsJsonAsync(path, body, TestContext.Current.CancellationToken);

    public async Task<HttpResponseMessage> RefreshAsync(string? refreshToken, bool withRequestedWithHeader = true)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/refresh");
        if (refreshToken is not null)
        {
            request.Headers.Add("Cookie", $"rt={refreshToken}");
        }

        if (withRequestedWithHeader)
        {
            request.Headers.Add("X-Requested-With", "fetch");
        }

        return await Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<HttpResponseMessage> LogoutAsync(string refreshToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/auth/logout");
        request.Headers.Add("Cookie", $"rt={refreshToken}");
        return await Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    public async Task<HttpResponseMessage> SendWithTokenAsync(HttpMethod method, string path, string? accessToken, object? body = null)
    {
        using var request = new HttpRequestMessage(method, path);
        if (accessToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await Client.SendAsync(request, TestContext.Current.CancellationToken);
    }

    /// <summary>The Set-Cookie header for the refresh cookie, or null if the response did not set one.</summary>
    public static string? RefreshSetCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.SingleOrDefault(v => v.StartsWith("rt=", StringComparison.Ordinal))
            : null;

    /// <summary>The token value inside a refresh Set-Cookie header.</summary>
    public static string RefreshTokenFrom(HttpResponseMessage response)
    {
        var header = RefreshSetCookie(response) ?? throw new InvalidOperationException("No refresh cookie was set.");
        return header["rt=".Length..header.IndexOf(';', StringComparison.Ordinal)];
    }

    /// <summary>A signed-in session: the response body plus the refresh token from the cookie.</summary>
    public sealed record SignedIn(AuthResponse Body, string RefreshToken)
    {
        public string AccessToken => Body.AccessToken;

        public static async Task<SignedIn> FromAsync(HttpResponseMessage response)
        {
            var body = await response.Content.ReadFromJsonAsync<AuthResponse>(Json, TestContext.Current.CancellationToken);
            return new SignedIn(body!, RefreshTokenFrom(response));
        }
    }
}
