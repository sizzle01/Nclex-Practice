using Nclex.Api.IntegrationTests.Infrastructure;

namespace Nclex.Api.IntegrationTests.Notifications;

/// <summary>
/// Imports a three-item set (two multiple choice with key b, one SATA with key a, b, e) and starts
/// it for a new verified user, ready to answer and submit.
/// </summary>
public sealed class ExamScenario(ApiFactory factory, AttemptsApi attempts)
{
    public const string Device = "c-1";

    private readonly CatalogContent _content = new(factory);

    public sealed record Started(string Token, Guid UserId, string Email, Guid AttemptId, string SetTitle);

    /// <param name="settings">Set settings, e.g. <c>new { revealMode = "exam", passThreshold = 0.5 }</c>.</param>
    public async Task<Started> StartAsync(object settings)
    {
        var (questions, setId) = (CatalogContent.NewIds(3), Guid.NewGuid());
        var title = $"Notify {setId:N}";
        await _content.ImportOkAsync(CatalogContent.FileWith(
            "RN",
            [
                CatalogContent.Question(questions[0], "SECE.MOC", "fundamentals"),
                CatalogContent.SataQuestion(questions[1]),
                CatalogContent.Question(questions[2], "PSI", "mental-health"),
            ],
            testSets: [CatalogContent.Set(setId, title, questions, settings)]));

        var user = await attempts.VerifiedUserAsync();
        var attempt = await attempts.StartOkAsync(user.AccessToken, setId);
        await attempts.OpenOkAsync(user.AccessToken, attempt.Id, Device);
        return new Started(user.AccessToken, user.Body.User.Id, user.Body.User.Email, attempt.Id, title);
    }

    /// <summary>Answers every item correctly.</summary>
    public async Task AnswerAllCorrectlyAsync(Started exam)
    {
        await attempts.SaveAsync(exam.Token, exam.AttemptId, 1, Device, new { selected = "b" }, 1);
        await attempts.SaveAsync(exam.Token, exam.AttemptId, 2, Device, new { selected = new[] { "a", "b", "e" } }, 1);
        await attempts.SaveAsync(exam.Token, exam.AttemptId, 3, Device, new { selected = "b" }, 1);
    }

    public Task<HttpResponseMessage> SubmitAsync(Started exam) => attempts.SubmitAsync(exam.Token, exam.AttemptId, Device);

    public async Task SubmitOkAsync(Started exam) => await AttemptsApi.JsonOkAsync(await SubmitAsync(exam));
}
