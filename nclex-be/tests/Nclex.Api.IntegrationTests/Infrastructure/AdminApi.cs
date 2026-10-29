using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Application.Admin.Questions;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Infrastructure;

/// <summary>
/// Staff accounts and admin-console calls for the F14 tests. A staff member is a new account whose
/// role is set in the database, then signed in again so the access token carries the role.
/// </summary>
public sealed class AdminApi(ApiFactory factory)
{
    private readonly AuthApi _auth = new(factory);

    /// <summary>A signed-in staff member of <paramref name="role"/>.</summary>
    public async Task<Staff> StaffAsync(UserRole role)
    {
        var email = AuthApi.NewEmail();
        var signedUp = await _auth.SignupNewUserAsync(email);
        await using (var scope = factory.Services.CreateAsyncScope())
        {
            await scope.ServiceProvider.GetRequiredService<AppDbContext>().Users
                .Where(u => u.Id == signedUp.Body.User.Id)
                .ExecuteUpdateAsync(u => u.SetProperty(x => x.Role, role), TestContext.Current.CancellationToken);
        }

        var signedIn = await _auth.LoginOkAsync(email);
        return new Staff(signedIn.Body.User.Id, email, signedIn.AccessToken);
    }

    public Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, Staff? who, object? body = null) =>
        _auth.SendWithTokenAsync(method, path, who?.AccessToken, body);

    /// <summary>Sends a request and reads the question view, failing the test unless it succeeds.</summary>
    public async Task<AdminQuestionView> QuestionOkAsync(HttpMethod method, string path, Staff who, object? body = null)
    {
        var response = await SendAsync(method, path, who, body);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"{method} {path} returned {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)}");
        }

        return (await response.Content.ReadFromJsonAsync<AdminQuestionView>(AuthApi.Json, TestContext.Current.CancellationToken))!;
    }

    /// <summary>Creates a multiple-choice question (options a to d, key b) with version 1 as a draft.</summary>
    public async Task<AdminQuestionView> CreateDraftAsync(Staff author, string stem = "Which action should the nurse take first?")
    {
        var draft = await McDraftAsync(stem);
        return await QuestionOkAsync(HttpMethod.Post, "/api/v1/admin/questions", author, new { track = "RN", draft });
    }

    /// <summary>Takes a new question through the whole workflow and returns it published.</summary>
    public async Task<AdminQuestionView> PublishedQuestionAsync(Staff author, Staff reviewer, string stem = "Which action should the nurse take first?")
    {
        var question = await CreateDraftAsync(author, stem);
        var versionId = question.Versions[0].Id;
        await QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/submit-for-review", author);
        await QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/approve", reviewer, new { notes = "Checked" });
        return await QuestionOkAsync(HttpMethod.Post, $"/api/v1/admin/question-versions/{versionId}/publish", reviewer);
    }

    /// <summary>A valid multiple-choice draft body for PUT or POST.</summary>
    public async Task<Dictionary<string, object?>> McDraftAsync(string stem = "Which action should the nurse take first?")
    {
        var (clientNeedsId, bodySystemId) = await TagIdsAsync();
        return new Dictionary<string, object?>
        {
            ["itemType"] = "multiple_choice",
            ["stem"] = stem,
            ["content"] = new { options = new[] { "a", "b", "c", "d" }.Select(o => new { id = o, text = $"Option {o}" }) },
            ["answerKey"] = new { correct = "b" },
            ["rationale"] = new { correct = "Why b", perOption = new { a = "A", b = "B", c = "C", d = "D" } },
            ["educationalObjective"] = "Prioritise safety.",
            ["exhibits"] = Array.Empty<object>(),
            ["clientNeedsId"] = clientNeedsId,
            ["bodySystemId"] = bodySystemId,
            ["difficulty"] = "easy",
        };
    }

    /// <summary>The ids of RN Client Needs leaf SECE.MOC and body system "fundamentals".</summary>
    public async Task<(short ClientNeedsId, short BodySystemId)> TagIdsAsync()
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var clientNeeds = await db.ClientNeedsCategories.Where(c => c.Track == ExamTrack.RN && c.Code == "SECE.MOC").Select(c => c.Id).SingleAsync(TestContext.Current.CancellationToken);
        var bodySystem = await db.BodySystems.Where(b => b.Slug == "fundamentals").Select(b => b.Id).SingleAsync(TestContext.Current.CancellationToken);
        return (clientNeeds, bodySystem);
    }

    /// <summary>Signed in as staff: the account's id and access token.</summary>
    public sealed record Staff(Guid Id, string Email, string AccessToken);
}
