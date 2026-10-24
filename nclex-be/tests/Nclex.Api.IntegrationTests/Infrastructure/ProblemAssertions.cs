using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Nclex.Api.IntegrationTests.Infrastructure;

public static class ProblemAssertions
{
    /// <summary>Asserts an api.md section 1 error body with the given status and code, and returns it.</summary>
    public static async Task<JsonElement> ShouldBeProblemAsync(this HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.ShouldBe(status);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(TestContext.Current.CancellationToken);
        body.GetProperty("code").GetString().ShouldBe(code);
        body.GetProperty("requestId").GetString().ShouldStartWith("req_");
        return body;
    }

    /// <summary>The (field, code) pairs of a VALIDATION_FAILED body.</summary>
    public static IReadOnlyList<(string Field, string Code)> FieldErrors(this JsonElement problem) =>
        [.. problem.GetProperty("errors").EnumerateArray()
            .Select(e => (e.GetProperty("field").GetString()!, e.GetProperty("code").GetString()!))];
}
