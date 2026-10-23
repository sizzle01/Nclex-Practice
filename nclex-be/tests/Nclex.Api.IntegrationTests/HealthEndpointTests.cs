using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests;

public sealed class HealthEndpointTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Healthz_returns_200_with_status_ok()
    {
        var response = await _client.GetAsync("/healthz", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        body.GetProperty("status").GetString().ShouldBe("ok");
    }

    [Fact]
    public async Task Readyz_returns_200_with_postgres_ok_and_redis_degraded_when_redis_is_down()
    {
        var response = await _client.GetAsync("/readyz", TestContext.Current.CancellationToken);

        // Redis is degraded-OK (api.md section 9), so the API is still ready.
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        body.GetProperty("status").GetString().ShouldBe("degraded");
        body.GetProperty("checks").GetProperty("postgres").GetString().ShouldBe("ok");
        body.GetProperty("checks").GetProperty("redis").GetString().ShouldBe("degraded");
    }
}
