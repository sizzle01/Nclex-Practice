using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Api.IntegrationTests.Notifications;

namespace Nclex.Api.IntegrationTests.Flags;

/// <summary>A candidate in an attempt of a fresh three-item set, plus the version ids they were served.</summary>
public sealed class FlagScenario(ApiFactory factory)
{
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));

    public sealed record Candidate(string Token, Guid UserId, Guid AttemptId, IReadOnlyList<Guid> VersionIds);

    public async Task<Candidate> CandidateInAttemptAsync()
    {
        var exam = await new ExamScenario(factory, _attempts).StartAsync(new { revealMode = "exam" });
        var versions = new List<Guid>();
        for (var position = 1; position <= 3; position++)
        {
            var item = await _attempts.GetItemOkAsync(exam.Token, exam.AttemptId, position);
            versions.Add(item.GetProperty("questionVersionId").GetGuid());
        }

        return new Candidate(exam.Token, exam.UserId, exam.AttemptId, versions);
    }
}
