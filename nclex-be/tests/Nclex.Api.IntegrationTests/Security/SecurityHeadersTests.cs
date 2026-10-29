using System.Net;
using System.Net.Http.Json;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Security;

/// <summary>Response headers and proxy trust from the F16 security pass (architecture.md section 6).</summary>
public sealed class SecurityHeadersTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Theory]
    [InlineData("/healthz")]
    [InlineData("/api/v1/me")] // 401: error bodies carry the headers too
    [InlineData(ThrowingController.Path)] // 500 written by the exception handler
    public async Task Every_response_carries_the_security_headers(string path)
    {
        var response = await _client.GetAsync(path, TestContext.Current.CancellationToken);

        Header(response, "X-Content-Type-Options").ShouldBe("nosniff");
        Header(response, "X-Frame-Options").ShouldBe("DENY");
        Header(response, "Referrer-Policy").ShouldBe("no-referrer");
        Header(response, "Content-Security-Policy").ShouldBe("default-src 'none'; frame-ancestors 'none'");
        Header(response, "Cross-Origin-Resource-Policy").ShouldBe("same-site");
    }

    [Fact]
    public async Task A_forwarded_address_is_believed_when_it_comes_through_a_trusted_proxy()
    {
        // Loopback is always trusted: locally that is the Next.js server forwarding to the API.
        var address = await AddressAsync(proxyIp: "127.0.0.1", forwardedFor: "203.0.113.7");

        address.Ip.ShouldBe("203.0.113.7");
        address.Scheme.ShouldBe("https");
    }

    [Fact]
    public async Task A_forwarded_address_from_an_unknown_client_is_ignored()
    {
        // Anyone can send X-Forwarded-For; believing it would let a client dodge the per-IP login limit.
        var address = await AddressAsync(proxyIp: "198.51.100.20", forwardedFor: "203.0.113.7");

        address.Ip.ShouldBe("198.51.100.20");
        address.Scheme.ShouldBe("http");
    }

    private async Task<ClientAddressController.ClientAddress> AddressAsync(string proxyIp, string forwardedFor)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, ClientAddressController.Path);
        request.Headers.Add(TestClientIpStartupFilter.Header, proxyIp);
        request.Headers.Add("X-Forwarded-For", forwardedFor);
        request.Headers.Add("X-Forwarded-Proto", "https");
        var response = await _client.SendAsync(request, TestContext.Current.CancellationToken);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ClientAddressController.ClientAddress>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    private static string? Header(HttpResponseMessage response, string name) =>
        response.Headers.TryGetValues(name, out var values) || response.Content.Headers.TryGetValues(name, out values)
            ? string.Join(",", values)
            : null;
}
