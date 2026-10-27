using Microsoft.Extensions.DependencyInjection;
using Nclex.Api.IntegrationTests.Infrastructure;
using Nclex.Application.Catalog;
using Nclex.Application.Catalog.CustomSets;
using Nclex.Application.Common;
using Nclex.Domain.Entities;
using Nclex.Domain.ItemTypes;

namespace Nclex.Api.IntegrationTests.Catalog;

/// <summary>
/// The builder's hand-written pool query against real Postgres. Other tests share the database,
/// so these check which of their own questions are in or out rather than total counts.
/// </summary>
public sealed class QuestionPoolTests(ApiFactory factory)
{
    private readonly AuthApi _api = new(factory);
    private readonly CatalogContent _content = new(factory);

    [Fact]
    public async Task A_parent_category_stands_for_all_its_leaves()
    {
        var (moc, sic, hpm) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.FileWith("RN",
        [
            CatalogContent.Question(moc, "SECE.MOC"),
            CatalogContent.Question(sic, "SECE.SIC"),
            CatalogContent.Question(hpm, "HPM"),
        ]));
        var sece = await _content.CategoryIdAsync(ExamTrack.RN, "SECE");

        var pool = await LoadAsync(await ViewerAsync(), new AvailableCountRequest(ClientNeedsIds: [sece]));

        pool.Items.Select(i => i.QuestionId).ShouldContain(moc);
        pool.Items.Select(i => i.QuestionId).ShouldContain(sic);
        pool.Items.Select(i => i.QuestionId).ShouldNotContain(hpm);
        pool.SelectedLeaves.Select(l => l.Weight).ShouldBe([18m, 13m]); // midpoints of 15-21% and 10-16%
    }

    [Fact]
    public async Task Body_system_difficulty_and_item_type_filters_narrow_the_pool()
    {
        var (match, otherSystem, otherDifficulty) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.FileWith("RN",
        [
            CatalogContent.Question(match, bodySystem: "cardiovascular", difficulty: "difficult"),
            CatalogContent.Question(otherSystem, bodySystem: "respiratory", difficulty: "difficult"),
            CatalogContent.Question(otherDifficulty, bodySystem: "cardiovascular", difficulty: "easy"),
        ]));
        var cardiovascular = await _content.BodySystemIdAsync("cardiovascular");
        var viewer = await ViewerAsync();

        var filtered = await LoadAsync(viewer, new AvailableCountRequest(
            BodySystemIds: [cardiovascular], Difficulties: [DifficultyTier.Difficult], ItemTypes: [ItemType.MultipleChoice]));
        var sataOnly = await LoadAsync(viewer, new AvailableCountRequest(BodySystemIds: [cardiovascular], ItemTypes: [ItemType.Sata]));

        var ids = filtered.Items.Select(i => i.QuestionId).ToList();
        ids.ShouldContain(match);
        ids.ShouldNotContain(otherSystem);
        ids.ShouldNotContain(otherDifficulty);
        sataOnly.Items.Select(i => i.QuestionId).ShouldNotContain(match);
    }

    [Fact]
    public async Task Premium_questions_are_only_in_a_premium_users_pool()
    {
        var premiumQuestion = Guid.NewGuid();
        await _content.ImportOkAsync(CatalogContent.FileWith("RN", [CatalogContent.Question(premiumQuestion, accessTier: "premium")]));
        var free = await ViewerAsync();
        var premium = free with { Plan = PlanTier.Premium };

        (await LoadAsync(free, new AvailableCountRequest())).Items.Select(i => i.QuestionId).ShouldNotContain(premiumQuestion);
        (await LoadAsync(premium, new AvailableCountRequest())).Items.Select(i => i.QuestionId).ShouldContain(premiumQuestion);
    }

    [Fact]
    public async Task Only_the_chosen_track_and_active_questions_are_used()
    {
        var (pn, retired) = (Guid.NewGuid(), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("PN", [pn]));
        await _content.ImportOkAsync(CatalogContent.File("RN", [retired]));
        await _content.RetireQuestionAsync(retired);
        var viewer = await ViewerAsync();

        var rn = (await LoadAsync(viewer, new AvailableCountRequest())).Items.Select(i => i.QuestionId).ToList();
        var pnPool = (await LoadAsync(viewer, new AvailableCountRequest(Track: ExamTrack.PN))).Items.Select(i => i.QuestionId).ToList();

        rn.ShouldNotContain(pn);
        rn.ShouldNotContain(retired);
        pnPool.ShouldContain(pn);
    }

    [Fact]
    public async Task Unused_incorrect_and_flagged_pools_follow_the_users_history()
    {
        var (correct, wrong, unseen) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.File("RN", [correct, wrong, unseen]));
        var viewer = await ViewerAsync();
        await _content.AddHistoryAsync(viewer.UserId, correct: true, correct);
        await _content.AddHistoryAsync(viewer.UserId, correct: false, wrong);

        var unused = await IdsAsync(viewer, PoolMode.Unused);
        var incorrect = await IdsAsync(viewer, PoolMode.Incorrect);
        var flagged = await IdsAsync(viewer, PoolMode.Flagged);

        unused.ShouldContain(unseen);
        unused.ShouldNotContain(correct);
        unused.ShouldNotContain(wrong);
        incorrect.ShouldBe([wrong]); // only this user's history counts
        flagged.ShouldBeEmpty(); // marking for review arrives with attempts (F08)
    }

    [Fact]
    public async Task A_case_study_is_used_only_when_all_its_items_pass_the_filters()
    {
        var (first, second, third) = (Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        await _content.ImportOkAsync(CatalogContent.FileWith("RN",
        [
            CatalogContent.Question(first, difficulty: "moderate"),
            CatalogContent.Question(second, difficulty: "moderate"),
            CatalogContent.Question(third, difficulty: "difficult"),
        ]));
        await _content.MakeCaseStudyAsync(first, second, third);
        var viewer = await ViewerAsync();

        var everything = await LoadAsync(viewer, new AvailableCountRequest());
        var moderateOnly = await LoadAsync(viewer, new AvailableCountRequest(Difficulties: [DifficultyTier.Moderate]));

        everything.Items.Where(i => i.QuestionId == first || i.QuestionId == second || i.QuestionId == third)
            .Select(i => i.CasePosition).ShouldBe([(short)1, (short)2, (short)3], ignoreOrder: true);
        moderateOnly.Items.Select(i => i.QuestionId).ShouldNotContain(first);
        moderateOnly.Items.Select(i => i.QuestionId).ShouldNotContain(second);
    }

    [Fact]
    public async Task Unknown_categories_and_body_systems_are_field_errors()
    {
        var viewer = await ViewerAsync();
        var pnLeaf = await _content.CategoryIdAsync(ExamTrack.PN, "SECE.CC");

        var category = await Should.ThrowAsync<AppException>(() => LoadAsync(viewer, new AvailableCountRequest(ClientNeedsIds: [pnLeaf])));
        var system = await Should.ThrowAsync<AppException>(() => LoadAsync(viewer, new AvailableCountRequest(BodySystemIds: [999])));

        category.FieldErrors.ShouldBe([new AppFieldError("clientNeedsIds", "INVALID_VALUE")]); // a PN leaf on the RN track
        system.FieldErrors.ShouldBe([new AppFieldError("bodySystemIds", "INVALID_VALUE")]);
    }

    private async Task<CatalogViewer> ViewerAsync()
    {
        var user = await _api.SignupNewUserAsync();
        return new CatalogViewer(user.Body.User.Id, ExamTrack.RN, PlanTier.Free);
    }

    private async Task<LoadedPool> LoadAsync(CatalogViewer viewer, AvailableCountRequest filters)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<QuestionPool>()
            .LoadAsync(filters, viewer, TestContext.Current.CancellationToken);
    }

    private async Task<List<Guid>> IdsAsync(CatalogViewer viewer, PoolMode pool) =>
        [.. (await LoadAsync(viewer, new AvailableCountRequest(Pool: pool))).Items.Select(i => i.QuestionId)];
}
