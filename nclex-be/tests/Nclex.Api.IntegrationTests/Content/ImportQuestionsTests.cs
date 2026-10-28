using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Content.ImportQuestions;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;
using Nclex.Infrastructure.Content;
using Nclex.Infrastructure.Persistence;

namespace Nclex.Api.IntegrationTests.Content;

public sealed class ImportQuestionsTests(ApiFactory factory)
{
    private static readonly string[] OptionIds = ["a", "b", "c", "d"];

    private static readonly string SampleFile = Path.Combine(AppContext.BaseDirectory, "content", "sample-questions.rn.json");

    [Fact]
    public async Task Sample_file_imports_and_importing_it_again_changes_nothing()
    {
        var file = QuestionImportFile.Parse(await File.ReadAllTextAsync(SampleFile, TestContext.Current.CancellationToken));
        var ids = file.Questions.Select(q => q.Id).ToList();

        // The media files sit in content/media, next to the file, as they do for the worker's command.
        var files = new DirectoryContentFiles(Path.GetDirectoryName(SampleFile)!);

        var first = await ImportAsync(file, files);
        var second = await ImportAsync(QuestionImportFile.Parse(await File.ReadAllTextAsync(SampleFile, TestContext.Current.CancellationToken)), files);

        first.ShouldBe(new ImportResult(Created: 40, Updated: 0, Unchanged: 0, [])
        {
            Banks = new ImportCounts(1, 0, 0),
            TestSets = new ImportCounts(3, 0, 0),
            Media = new ImportCounts(2, 0, 0),
            CaseStudies = new ImportCounts(1, 0, 0),
        }, "first import");
        second.ShouldBe(new ImportResult(Created: 0, Updated: 0, Unchanged: 40, [])
        {
            Banks = new ImportCounts(0, 0, 1),
            TestSets = new ImportCounts(0, 0, 3),
            Media = new ImportCounts(0, 0, 2),
            CaseStudies = new ImportCounts(0, 0, 1),
        }, "second import");

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var versions = await db.QuestionVersions.Where(v => ids.Contains(v.QuestionId)).ToListAsync(TestContext.Current.CancellationToken);
        versions.Count.ShouldBe(40);
        versions.ShouldAllBe(v => v.Status == VersionStatus.Published && v.Version == 1);
        versions.Count(v => v.ItemType == ItemType.MultipleChoice).ShouldBe(10);
        versions.Where(v => v.ItemType == ItemType.MultipleChoice).ShouldAllBe(v => v.MaxPoints == 1);
        versions.Where(v => v.ItemType == ItemType.Sata).ShouldAllBe(v => v.MaxPoints > 1);

        // Two samples of every other MVP type (F05), with max points worked out by each type's module.
        var otherMvpTypes = ItemTypeCatalog.All
            .Where(d => d.Phase == Phase.Mvp && d.Type is not (ItemType.MultipleChoice or ItemType.Sata))
            .Select(d => d.Type);
        versions.GroupBy(v => v.ItemType).Where(g => otherMvpTypes.Contains(g.Key))
            .Select(g => (g.Key, g.Count())).ShouldBe(otherMvpTypes.Select(t => (t, 2)), ignoreOrder: true);
        versions.Where(v => v.ItemType == ItemType.SelectN).Select(v => (int)v.MaxPoints).ShouldBe([3, 1], ignoreOrder: true); // per option, then all_or_nothing
        versions.Where(v => v.ItemType == ItemType.OrderedResponse).Select(v => (int)v.MaxPoints).ShouldBe([1, 5], ignoreOrder: true); // all_or_nothing, then per_position

        var questions = await db.Questions.Where(q => ids.Contains(q.Id)).ToListAsync(TestContext.Current.CancellationToken);
        questions.ShouldAllBe(q => versions.Any(v => v.Id == q.CurrentVersionId));

        // F06: the three curated sets of PRD 5.5 and one bank, all built from the file's questions.
        var sets = await db.TestSets.Where(s => file.TestSets!.Select(t => t.Id).Contains(s.Id))
            .OrderBy(s => s.Title).ToListAsync(TestContext.Current.CancellationToken);
        sets.Select(s => (s.Title, s.QuestionCount, s.Mode, s.RevealMode, s.Timed, s.TimeBudgetSec, s.PassThreshold, s.IsSample)).ShouldBe(
        [
            ("Comprehensive Challenge 1", 40, TestMode.Challenge, RevealMode.Exam, true, (int?)4800, (decimal?)0.65m, false),
            ("Comprehensive Practice 1", 40, TestMode.Practice, RevealMode.Tutor, false, null, null, false),
            ("RN Sample Set", 10, TestMode.Practice, RevealMode.Tutor, false, null, null, true),
        ]);
        sets.ShouldAllBe(s => s.Kind == TestSetKind.Prebuilt && s.IsPublished && s.AccessTier == PlanTier.Free && s.Track == ExamTrack.RN);
        var bank = await db.QuestionBanks.SingleAsync(b => b.Id == file.Banks![0].Id, TestContext.Current.CancellationToken);
        bank.Name.ShouldBe("RN Sample Bank");
        (await db.QuestionBankItems.CountAsync(i => i.BankId == bank.Id, TestContext.Current.CancellationToken)).ShouldBe(40);

        // F11: five questions carry exhibits, two of them showing the file's image and audio clip.
        versions.Count(v => v.Exhibits.RootElement.GetArrayLength() > 0).ShouldBe(5);
        var media = await db.MediaAssets.Where(m => file.Media!.Select(x => x.Id).Contains(m.Id)).ToListAsync(TestContext.Current.CancellationToken);
        media.Select(m => (m.Kind, m.MimeType, m.AltText is not null, m.CaptionsKey is not null, m.TranscriptKey is not null)).ShouldBe(
            [("image", "image/png", true, false, false), ("audio", "audio/wav", false, true, true)], ignoreOrder: true);
        media.ShouldAllBe(m => File.Exists(Path.Combine(factory.MediaRoot, m.StorageKey)));
        media.Where(m => m.TranscriptKey != null).ShouldAllBe(m => File.Exists(Path.Combine(factory.MediaRoot, m.TranscriptKey!)));

        // F11: one case study, whose three heart-failure questions sit back to back in both comprehensive sets.
        var caseStudy = await db.CaseStudies.SingleAsync(c => c.Id == file.CaseStudies![0].Id, TestContext.Current.CancellationToken);
        caseStudy.Title.ShouldBe("Heart failure exacerbation");
        caseStudy.Scenario.RootElement.GetProperty("tabs").GetArrayLength().ShouldBe(4);
        versions.Where(v => v.CaseStudyId == caseStudy.Id).OrderBy(v => v.CasePosition)
            .Select(v => v.ItemType).ShouldBe([ItemType.Sata, ItemType.MatrixMc, ItemType.MultipleResponseGrouping]);

        var importer = await db.Users.SingleAsync(u => u.Email == User.ContentImporterEmail, TestContext.Current.CancellationToken);
        importer.Role.ShouldBe(UserRole.Author);
        importer.PasswordHash.ShouldBeNull();
        versions.ShouldAllBe(v => v.AuthorId == importer.Id && v.ReviewNotes == QuestionVersion.ImportedReviewNote);
    }

    [Fact]
    public async Task A_file_with_a_missing_option_rationale_writes_nothing()
    {
        var valid = McQuestion(Guid.NewGuid());
        var missingRationale = McQuestion(Guid.NewGuid(), rationale: new { correct = "Why", perOption = new { a = "A", b = "B", c = "C" } });

        var result = await ImportAsync(ImportFile(valid, missingRationale));

        result.Succeeded.ShouldBeFalse();
        result.Issues.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            i => i.Path.ShouldBe("questions[1].rationale.perOption.d"),
            i => i.Code.ShouldBe(ValidationIssueCodes.MissingRationale));
        (await CountQuestionsAsync(valid.Id, missingRationale.Id)).ShouldBe(0);
    }

    [Fact]
    public async Task Unknown_tags_unsupported_types_and_repeated_ids_are_reported()
    {
        var id = Guid.NewGuid();
        var file = ImportFile(
            McQuestion(id, clientNeeds: "SECE"),
            McQuestion(id, bodySystem: "spleen"),
            McQuestion(Guid.NewGuid(), itemType: "bowtie")); // phase 2: no module yet

        var result = await ImportAsync(file);

        result.Issues.Select(i => (i.Path, i.Code)).ShouldBe(
        [
            ("questions[0].clientNeeds", ImportIssueCodes.UnknownTag), // a parent category, not a leaf
            ("questions[1].id", ImportIssueCodes.DuplicateId),
            ("questions[1].bodySystem", ImportIssueCodes.UnknownTag),
            ("questions[2].itemType", ImportIssueCodes.UnsupportedItemType),
        ]);
    }

    [Fact]
    public async Task A_changed_question_gets_a_new_version_that_supersedes_the_old_one()
    {
        var id = Guid.NewGuid();
        await ImportAsync(ImportFile(McQuestion(id)));

        var result = await ImportAsync(ImportFile(McQuestion(id, stem: "Which action is safest?")));

        result.ShouldBe(new ImportResult(Created: 0, Updated: 1, Unchanged: 0, []));
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var versions = await db.QuestionVersions.Where(v => v.QuestionId == id).OrderBy(v => v.Version)
            .ToListAsync(TestContext.Current.CancellationToken);
        versions.Select(v => (v.Version, v.Status)).ShouldBe([(1, VersionStatus.Superseded), (2, VersionStatus.Published)]);
        versions[1].Stem.ShouldBe("Which action is safest?");
        (await db.Questions.SingleAsync(q => q.Id == id, TestContext.Current.CancellationToken)).CurrentVersionId.ShouldBe(versions[1].Id);
    }

    [Fact]
    public async Task Changing_only_the_access_tier_updates_the_question_without_a_new_version()
    {
        var id = Guid.NewGuid();
        await ImportAsync(ImportFile(McQuestion(id)));

        var result = await ImportAsync(ImportFile(McQuestion(id, accessTier: "premium")));

        result.Updated.ShouldBe(1);
        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.Questions.SingleAsync(q => q.Id == id, TestContext.Current.CancellationToken)).AccessTier.ShouldBe(PlanTier.Premium);
        (await db.QuestionVersions.CountAsync(v => v.QuestionId == id, TestContext.Current.CancellationToken)).ShouldBe(1);
    }

    [Fact]
    public async Task A_question_cannot_move_to_the_other_track()
    {
        var id = Guid.NewGuid();
        await ImportAsync(ImportFile(McQuestion(id)));

        var result = await ImportAsync(ImportFile("PN", McQuestion(id, clientNeeds: "SECE.CC")));

        result.Issues.ShouldHaveSingleItem().Code.ShouldBe(ImportIssueCodes.TrackMismatch);
    }

    // Through CatalogContent, so it waits its turn with every other test class's imports.
    private Task<ImportResult> ImportAsync(QuestionImportFile file, IContentFiles? files = null) => new CatalogContent(factory).ImportAsync(file, files);

    private async Task<int> CountQuestionsAsync(params Guid[] ids)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<AppDbContext>().Questions
            .CountAsync(q => ids.Contains(q.Id), TestContext.Current.CancellationToken);
    }

    // Files are built as JSON and parsed, so these tests also cover the file format.
    private static QuestionImportFile ImportFile(params TestQuestion[] questions) => ImportFile("RN", questions);

    private static QuestionImportFile ImportFile(string track, params TestQuestion[] questions) =>
        QuestionImportFile.Parse(JsonSerializer.Serialize(new { track, questions = questions.Select(q => q.Json) }));

    private static TestQuestion McQuestion(
        Guid id,
        string stem = "Which action should the nurse take first?",
        string itemType = "multiple_choice",
        string clientNeeds = "SECE.MOC",
        string bodySystem = "fundamentals",
        string accessTier = "free",
        object? rationale = null) =>
        new(id, new Dictionary<string, object>
        {
            ["id"] = id,
            ["itemType"] = itemType,
            ["stem"] = stem,
            ["content"] = new { options = OptionIds.Select(o => new { id = o, text = $"Option {o}" }) },
            ["key"] = new { correct = "b" },
            ["rationale"] = rationale ?? new { correct = "Why b", perOption = new { a = "A", b = "B", c = "C", d = "D" } },
            ["educationalObjective"] = "Prioritise safety.",
            ["clientNeeds"] = clientNeeds,
            ["bodySystem"] = bodySystem,
            ["difficulty"] = "easy",
            ["accessTier"] = accessTier,
        });

    private sealed record TestQuestion(Guid Id, Dictionary<string, object> Json);
}
