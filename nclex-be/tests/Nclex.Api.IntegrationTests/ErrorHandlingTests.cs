using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Api.Middleware;

namespace Nclex.Api.IntegrationTests;

public sealed class ErrorHandlingTests(ApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Unhandled_exception_returns_500_problem_details_with_request_id()
    {
        var response = await _client.GetAsync(ThrowingController.Path, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");

        var raw = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        raw.ShouldNotContain(ThrowingController.SecretMessage);

        var body = JsonSerializer.Deserialize<JsonElement>(raw);
        body.GetProperty("status").GetInt32().ShouldBe(500);
        body.GetProperty("code").GetString().ShouldBe("INTERNAL_ERROR");
        body.GetProperty("type").GetString().ShouldBe("https://errors.nclex-practice.app/internal-error");
        body.GetProperty("title").GetString().ShouldNotBeNullOrWhiteSpace();

        var requestId = body.GetProperty("requestId").GetString();
        requestId.ShouldNotBeNull();
        requestId.ShouldStartWith("req_");
        response.Headers.GetValues(RequestIdMiddleware.HeaderName).ShouldHaveSingleItem().ShouldBe(requestId);
    }

    [Fact]
    public async Task Unknown_route_returns_404_problem_details_with_not_found_code()
    {
        var response = await _client.GetAsync("/api/v1/does-not-exist", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        body.GetProperty("code").GetString().ShouldBe("NOT_FOUND");
        body.GetProperty("requestId").GetString().ShouldStartWith("req_");
    }
}
