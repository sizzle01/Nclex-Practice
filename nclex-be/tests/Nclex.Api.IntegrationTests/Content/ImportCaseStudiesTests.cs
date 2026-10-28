using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Domain.ItemTypes;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Content;

/// <summary>Case studies in import files (F11): the case_studies rows, questions joining them, and the checks.</summary>
public sealed class ImportCaseStudiesTests(ApiFactory factory)
{
    private const string Device = "c-1";

    private readonly CatalogContent _content = new(factory);
    private readonly AttemptsApi _attempts = new(factory, new AuthApi(factory));
    private readonly Guid _caseId = Guid.NewGuid();
    private readonly List<Guid> _ids = CatalogContent.NewIds(3);

    [Fact]
    public async Task A_case_and_its_questions_are_stored_and_importing_again_changes_nothing()
    {
        var first = await _content.ImportAsync(CaseFile());
        var second = await _content.ImportAsync(CaseFile());

        first.Issues.ShouldBeEmpty();
        first.CaseStudies.ShouldBe(new ImportCounts(1, 0, 0));
        second.CaseStudies.ShouldBe(new ImportCounts(0, 0, 1));
        second.Unchanged.ShouldBe(3);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var caseStudy = await db.CaseStudies.SingleAsync(c => c.Id == _caseId, TestContext.Current.CancellationToken);
        caseStudy.Title.ShouldBe("Heart failure");
        caseStudy.Track.ShouldBe(Nclex.Domain.Entities.ExamTrack.RN);
        caseStudy.Scenario.RootElement.GetProperty("tabs")[0].GetProperty("id").GetString().ShouldBe("notes");
        var versions = await db.QuestionVersions.Where(v => _ids.Contains(v.QuestionId)).ToListAsync(TestContext.Current.CancellationToken);
        versions.OrderBy(v => v.CasePosition).Select(v => (v.QuestionId, v.CaseStudyId, v.CasePosition))
            .ShouldBe(_ids.Select((id, i) => (id, (Guid?)_caseId, (short?)(i + 1))));
    }

    [Fact]
    public async Task Changing_the_scenario_updates_the_case_in_place()
    {
        await _content.ImportOkAsync(CaseFile());

        var result = await _content.ImportAsync(CaseFile(scenario: Scenario(noteBody: "Now short of breath at rest.")));

        result.CaseStudies.ShouldBe(new ImportCounts(0, 1, 0));
        result.Unchanged.ShouldBe(3);
    }

    [Fact]
    public async Task A_question_can_join_a_case_imported_by_an_earlier_file()
    {
        await _content.ImportOkAsync(CaseFile());
        var later = Guid.NewGuid();
        var question = CatalogContent.Question(later);
        question["caseStudyId"] = _caseId;
        question["casePosition"] = 4;

        var result = await _content.ImportAsync(CatalogContent.FileWith("RN", [question]));

        result.Issues.ShouldBeEmpty();
        result.CaseStudies.ShouldBe(ImportCounts.None);
    }

    [Fact]
    public async Task Leaving_a_case_gives_the_question_a_new_version()
    {
        await _content.ImportOkAsync(CaseFile());

        // The last item leaves, so positions 1 and 2 remain with no gap.
        var result = await _content.ImportAsync(CaseFile(placements: [1, 2, null]));

        result.Issues.ShouldBeEmpty();
        result.Updated.ShouldBe(1);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var current = await db.Questions.Where(q => q.Id == _ids[2])
            .Join(db.QuestionVersions, q => q.CurrentVersionId, v => v.Id, (q, v) => v)
            .SingleAsync(TestContext.Current.CancellationToken);
        current.ShouldSatisfyAllConditions(
            v => v.Version.ShouldBe(2),
            v => v.CaseStudyId.ShouldBeNull(),
            v => v.CasePosition.ShouldBeNull());
    }

    [Fact]
    public async Task Leaving_a_gap_in_a_case_already_imported_is_refused()
    {
        await _content.ImportOkAsync(CaseFile());
        var middle = CatalogContent.Question(_ids[1]);

        var result = await _content.ImportAsync(CatalogContent.FileWith("RN", [middle]));

        result.Issues.ShouldContain(i => i.Path == "questions[0].caseStudyId" && i.Code == ImportIssueCodes.InvalidValue);
    }

    public static TheoryData<string, string, string> BrokenCases => new()
    {
        { "unknown case", "questions[0].caseStudyId", ImportIssueCodes.UnknownCaseStudy },
        { "position without a case", "questions[0].caseStudyId", ImportIssueCodes.Required },
        { "case without a position", "questions[0].casePosition", ImportIssueCodes.Required },
        { "position 7", "questions[0].casePosition", ImportIssueCodes.InvalidValue },
        { "repeated position", "caseStudies[0].id", ImportIssueCodes.InvalidValue },
        { "gap in positions", "caseStudies[0].id", ImportIssueCodes.InvalidValue },
        { "no tabs", "caseStudies[0].scenario.tabs", ValidationIssueCodes.TooFew },
        { "scenario not an object", "caseStudies[0].scenario", ValidationIssueCodes.InvalidShape },
        { "malformed exhibit in a tab", "caseStudies[0].scenario.tabs[0].exhibits[0].body", ValidationIssueCodes.Required },
        { "unknown media in a tab", "caseStudies[0].scenario.tabs[0].exhibits[0].mediaId", ImportIssueCodes.UnknownMedia },
        { "exhibit id that is also a tab id", "questions[0].exhibits[0].id", ValidationIssueCodes.DuplicateId },
        { "no title", "caseStudies[0].title", ImportIssueCodes.Required },
    };

    [Theory]
    [MemberData(nameof(BrokenCases))]
    public async Task A_case_problem_writes_nothing(string problem, string path, string code)
    {
        var file = problem switch
        {
            "unknown case" => CaseFile(caseStudyIdOfFirst: Guid.NewGuid()),
            "position without a case" => CaseFile(caseStudyIdOfFirst: Guid.Empty),
            "case without a position" => CaseFile(placements: [null, 2, 3], keepCaseOnFirst: true),
            "position 7" => CaseFile(placements: [7, 2, 3]),
            "repeated position" => CaseFile(placements: [1, 1, 2]),
            "gap in positions" => CaseFile(placements: [1, 2, 4]),
            "no tabs" => CaseFile(scenario: new { tabs = Array.Empty<object>() }),
            "scenario not an object" => CaseFile(scenario: new[] { "notes" }),
            "malformed exhibit in a tab" => CaseFile(scenario: new { tabs = new[] { new { id = "notes", title = "Notes", exhibits = new[] { new { id = "n", kind = "text", title = "Note" } } } } }),
            "unknown media in a tab" => CaseFile(scenario: new { tabs = new[] { new { id = "photo", title = "Photo", exhibits = new[] { new { id = "p", kind = "image", title = "Wound", mediaId = Guid.NewGuid() } } } } }),
            "exhibit id that is also a tab id" => CaseFile(exhibitOnFirst: new { id = "notes", kind = "text", title = "Note", body = "Calm." }),
            _ => CaseFile(title: " "),
        };

        var result = await _content.ImportAsync(file);

        result.Issues.ShouldContain(i => i.Path == path && i.Code == code, problem);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.CaseStudies.AnyAsync(c => c.Id == _caseId, TestContext.Current.CancellationToken)).ShouldBeFalse();
        (await db.Questions.AnyAsync(q => _ids.Contains(q.Id), TestContext.Current.CancellationToken)).ShouldBeFalse();
    }

    public static TheoryData<string, int[]> SplitSets => new()
    {
        { "only some of the case", [0, 1] },
        { "out of case order", [1, 0, 2] },
        { "not back to back", [0, 1, -1, 2] },
    };

    [Theory]
    [MemberData(nameof(SplitSets))]
    public async Task A_curated_set_must_keep_a_case_whole_and_in_order(string problem, int[] order)
    {
        var other = Guid.NewGuid();
        var setIds = order.Select(i => i < 0 ? other : _ids[i]);
        var file = CaseFile(extraQuestion: other, testSets: [CatalogContent.Set(Guid.NewGuid(), $"Case set {Guid.NewGuid():N}", setIds)]);

        var result = await _content.ImportAsync(file);

        result.Issues.ShouldContain(i => i.Path.StartsWith("testSets[0].questionIds[", StringComparison.Ordinal) && i.Code == ImportIssueCodes.CaseStudySplit, problem);
    }

    [Fact]
    public async Task An_item_in_a_case_brings_the_case_to_the_candidate()
    {
        var setId = Guid.NewGuid();
        await _content.ImportOkAsync(CaseFile(testSets: [CatalogContent.Set(setId, $"Case {setId:N}", _ids)]));
        var user = (await _attempts.VerifiedUserAsync()).AccessToken;
        var attempt = await _attempts.StartOkAsync(user, setId);
        await _attempts.OpenOkAsync(user, attempt.Id, Device);

        var item = await _attempts.GetItemOkAsync(user, attempt.Id, 2);

        var caseStudy = item.GetProperty("caseStudy");
        caseStudy.GetProperty("id").GetGuid().ShouldBe(_caseId);
        caseStudy.GetProperty("title").GetString().ShouldBe("Heart failure");
        caseStudy.GetProperty("position").GetInt32().ShouldBe(2);
        caseStudy.GetProperty("scenario").GetProperty("tabs").EnumerateArray()
            .Select(t => t.GetProperty("title").GetString()).ShouldBe(["Nurses' notes", "Vital signs"]);
    }

    private static object Scenario(string noteBody = "Short of breath when lying flat.") => new
    {
        tabs = new object[]
        {
            new { id = "notes", title = "Nurses' notes", exhibits = new[] { new { id = "note", kind = "text", title = "0800", body = noteBody } } },
            new
            {
                id = "vitals",
                title = "Vital signs",
                exhibits = new[] { new { id = "vs", kind = "flowsheet", title = "Vital signs", table = new { columns = new[] { "0800" }, rows = new[] { new { label = "HR", cells = new[] { "112" } } } } } },
            },
        },
    };

    // An RN file with the case and its three questions at positions 1..3 unless told otherwise.
    private QuestionImportFile CaseFile(
        object? scenario = null,
        string title = "Heart failure",
        int?[]? placements = null,
        Guid? caseStudyIdOfFirst = null,
        bool keepCaseOnFirst = false,
        object? exhibitOnFirst = null,
        Guid? extraQuestion = null,
        IEnumerable<object>? testSets = null)
    {
        placements ??= [1, 2, 3];
        var questions = _ids.Select((id, i) =>
        {
            var question = CatalogContent.Question(id);
            if (placements[i] is { } position)
            {
                question["caseStudyId"] = _caseId;
                question["casePosition"] = position;
            }
            else if (keepCaseOnFirst && i == 0)
            {
                question["caseStudyId"] = _caseId;
            }

            return question;
        }).ToList();

        if (caseStudyIdOfFirst is { } caseId)
        {
            // Guid.Empty stands for "no case id", leaving the position on its own.
            questions[0].Remove("caseStudyId");
            if (caseId != Guid.Empty)
            {
                questions[0]["caseStudyId"] = caseId;
            }
        }

        if (exhibitOnFirst is not null)
        {
            questions[0]["exhibits"] = new[] { exhibitOnFirst };
        }

        if (extraQuestion is { } extra)
        {
            questions.Add(CatalogContent.Question(extra));
        }

        return QuestionImportFile.Parse(JsonSerializer.Serialize(new
        {
            track = "RN",
            questions,
            caseStudies = new[] { new { id = _caseId, title, scenario = scenario ?? Scenario() } },
            testSets = testSets ?? [],
        }));
    }
}
