using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Domain.Audit;
using Nclex.Domain.Entities;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Content;

/// <summary>
/// Retiring questions through an import file (F26): the case-study conversion takes old isolated questions out of
/// practice once their topic is rebuilt as case studies.
/// </summary>
public sealed class ImportRetiredQuestionsTests(ApiFactory factory)
{
    private const string Device = "retire-1";

    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));
    private readonly List<Guid> _ids = CatalogContent.NewIds(3);

    [Fact]
    public async Task A_question_marked_retired_is_retired_and_audited()
    {
        await _content.ImportOkAsync(CatalogContent.FileWith("RN", [.. _ids.Select(id => CatalogContent.Question(id))]));

        var result = await _content.ImportAsync(CatalogContent.FileWith("RN", [Retired(_ids[0]), CatalogContent.Question(_ids[1])]));

        result.Issues.ShouldBeEmpty();
        result.Retired.ShouldBe(1);
        result.Unchanged.ShouldBe(2);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var question = await db.Questions.SingleAsync(q => q.Id == _ids[0], TestContext.Current.CancellationToken);
        question.ShouldSatisfyAllConditions(
            q => q.Status.ShouldBe(QuestionStatus.Retired),
            q => q.RetiredAt.ShouldNotBeNull(),
            q => q.CurrentVersionId.ShouldNotBeNull());
        (await db.AuditEvents.AnyAsync(a => a.Action == AuditActions.QuestionRetired && a.EntityId == _ids[0].ToString(),
            TestContext.Current.CancellationToken)).ShouldBeTrue();
    }

    [Fact]
    public async Task Importing_again_without_the_status_never_brings_a_question_back()
    {
        await _content.ImportOkAsync(CatalogContent.FileWith("RN", [Retired(_ids[0])]));

        var again = await _content.ImportAsync(CatalogContent.FileWith("RN", [CatalogContent.Question(_ids[0])]));
        var retiredAgain = await _content.ImportAsync(CatalogContent.FileWith("RN", [Retired(_ids[0])]));

        again.Retired.ShouldBe(0);
        retiredAgain.Retired.ShouldBe(0);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Questions.SingleAsync(q => q.Id == _ids[0], TestContext.Current.CancellationToken)).IsRetired.ShouldBeTrue();
    }

    [Fact]
    public async Task A_file_cannot_retire_a_question_and_list_it_in_a_set()
    {
        var file = CatalogContent.FileWith("RN", [Retired(_ids[0]), CatalogContent.Question(_ids[1])],
            testSets: [CatalogContent.Set(Guid.NewGuid(), $"Retire check {Guid.NewGuid():N}", [_ids[0], _ids[1]])]);

        var result = await _content.ImportAsync(file);

        result.Issues.ShouldContain(i => i.Path == "testSets[0].questionIds[0]" && i.Code == ImportIssueCodes.RetiredQuestion);
    }

    [Fact]
    public async Task A_set_imported_earlier_leaves_out_a_question_retired_later()
    {
        var setId = Guid.NewGuid();
        await _content.ImportOkAsync(CatalogContent.FileWith("RN", [.. _ids.Select(id => CatalogContent.Question(id))],
            testSets: [CatalogContent.Set(setId, $"Before retirement {setId:N}", _ids)]));
        await _content.ImportOkAsync(CatalogContent.FileWith("RN", [Retired(_ids[1])]));
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;

        var attempt = await _attempts.StartOkAsync(user, setId);

        attempt.TotalItems.ShouldBe(2);
    }

    private static Dictionary<string, object> Retired(Guid id)
    {
        var question = CatalogContent.Question(id);
        question["status"] = "retired";
        return question;
    }
}
